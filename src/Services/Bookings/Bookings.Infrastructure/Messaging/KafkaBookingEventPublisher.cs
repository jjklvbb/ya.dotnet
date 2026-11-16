using System.Text.Json;
using Bookings.Application.Ports;
using Confluent.Kafka;
using Messaging.Contracts;
using Messaging.Contracts.Events;
using Microsoft.Extensions.Configuration;

namespace Bookings.Infrastructure.Messaging;

public sealed class KafkaBookingEventPublisher
    : IBookingEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaBookingEventPublisher(IConfiguration configuration)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException(
                "Не настроен адрес Kafka.");

        _producer = new ProducerBuilder<string, string>(
            new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All
            }).Build();
    }

    public async Task PublishConfirmedAsync(
        Guid bookingId,
        Guid eventId,
        Guid userId,
        int seatsCount,
        DateTime confirmedAt,
        CancellationToken cancellationToken = default)
    {
        var payload = new BookingConfirmed(
            bookingId,
            eventId,
            userId,
            seatsCount,
            confirmedAt);

        await _producer.ProduceAsync(
            KafkaTopics.BookingConfirmed,
            new Message<string, string>
            {
                Key = eventId.ToString(),
                Value = JsonSerializer.Serialize(payload)
            },
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}