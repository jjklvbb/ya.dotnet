using System.Text.Json;
using Confluent.Kafka;
using Events.Infrastructure.DataAccess;
using Events.Infrastructure.Messaging;
using Messaging.Contracts;
using Messaging.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Infrastructure.Messaging;

public sealed class BookingConfirmedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookingConfirmedConsumer> _logger;

    public BookingConfirmedConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<BookingConfirmedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await Task.Yield();

        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"]
                ?? throw new InvalidOperationException(
                    "Не настроен адрес Kafka."),
            GroupId = "events-booking-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .Build();

        consumer.Subscribe(KafkaTopics.BookingConfirmed);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? message;

                try
                {
                    message = consumer.Consume(stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Ошибка чтения Kafka");
                    await Task.Delay(2000, stoppingToken);
                    continue;
                }

                if (message is null)
                    continue;

                try
                {
                    BookingConfirmed? booking;

                    try
                    {
                        booking = JsonSerializer.Deserialize<BookingConfirmed>(
                            message.Message.Value);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(
                            ex, "Некорректный JSON в Kafka");

                        consumer.Commit(message);
                        continue;
                    }

                    if (booking is null ||
                        booking.BookingId == Guid.Empty ||
                        booking.EventId == Guid.Empty)
                    {
                        _logger.LogWarning(
                            "Некорректное сообщение Kafka: {Offset}",
                            message.Offset);

                        consumer.Commit(message);
                        continue;
                    }

                    await ProcessBookingAsync(booking, stoppingToken);

                    consumer.Commit(message);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Ошибка обработки сообщения Kafka, offset {Offset}",
                        message.Offset);

                    // Повторно читаем сообщение после ошибки.
                    consumer.Seek(message.TopicPartitionOffset);

                    await Task.Delay(2000, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Kafka consumer остановлен");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessBookingAsync(
        BookingConfirmed message,
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<EventsDbContext>();

        await using var transaction =
            await db.Database.BeginTransactionAsync(ct);

        var alreadyProcessed = await db.ProcessedBookingMessages
            .AnyAsync(x => x.BookingId == message.BookingId, ct);

        if (alreadyProcessed)
        {
            _logger.LogInformation(
                "Бронь {BookingId} уже обработана",
                message.BookingId);

            return;
        }

        var ev = await db.Events
            .FirstOrDefaultAsync(x => x.Id == message.EventId, ct);

        if (ev is null)
        {
            _logger.LogWarning(
                "Бронь {BookingId} пропущена: мероприятие {EventId} не найдено",
                message.BookingId,
                message.EventId);

            return;
        }

        if (!ev.TryReserveSeats())
        {
            _logger.LogWarning(
                "Бронь {BookingId} пропущена: нет свободных мест для мероприятия {EventId}",
                message.BookingId,
                message.EventId);

            return;
        }

        db.ProcessedBookingMessages.Add(
            new ProcessedBookingMessage
            {
                BookingId = message.BookingId,
                ProcessedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        _logger.LogInformation(
            "Бронь {BookingId} обработана, место списано",
            message.BookingId);
    }
}