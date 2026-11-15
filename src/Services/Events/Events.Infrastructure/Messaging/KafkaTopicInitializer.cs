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
            ?? throw new InvalidOperationException("Не настроен адрес Kafka.");

        using var admin = new AdminClientBuilder(
            new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            }).Build();

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
        }
        catch (CreateTopicsException ex)
            when (ex.Results.All(x =>
                x.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            _logger.LogInformation(
                "Kafka-топик {Topic} уже существует",
                KafkaTopics.BookingConfirmed);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Не удалось создать Kafka-топик {Topic}",
                KafkaTopics.BookingConfirmed);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
