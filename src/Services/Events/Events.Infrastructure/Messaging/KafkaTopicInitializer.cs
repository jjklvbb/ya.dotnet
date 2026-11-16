using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Messaging.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace Events.Infrastructure.Messaging;

public sealed class KafkaTopicInitializer : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaTopicInitializer> _logger;

    public KafkaTopicInitializer(
        IConfiguration configuration,
        ILogger<KafkaTopicInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException(
                "Не настроен адрес Kafka.");

        const int maxAttempts = 10;

        using var admin = new AdminClientBuilder(
            new AdminClientConfig
            {
                BootstrapServers = bootstrapServers,
                SocketTimeoutMs = 5000
            }).Build();

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await admin.CreateTopicsAsync(
                [
                    new TopicSpecification
                {
                    Name = KafkaTopics.BookingConfirmed,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
                ]);

                _logger.LogInformation(
                    "Kafka-топик {Topic} создан",
                    KafkaTopics.BookingConfirmed);

                return;
            }
            catch (CreateTopicsException ex)
                when (ex.Results.All(x =>
                    x.Error.Code == ErrorCode.TopicAlreadyExists))
            {
                _logger.LogInformation(
                    "Kafka-топик {Topic} уже существует",
                    KafkaTopics.BookingConfirmed);

                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Не удалось создать Kafka-топик. Попытка {Attempt} из {MaxAttempts}",
                    attempt,
                    maxAttempts);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }


    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
