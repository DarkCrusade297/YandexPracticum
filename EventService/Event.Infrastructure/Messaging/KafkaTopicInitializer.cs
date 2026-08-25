using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Messaging.Contracts.Bookings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Event.Infrastructure.Messaging;

public sealed class KafkaTopicInitializer(
    IOptions<KafkaOptions> options,
    ILogger<KafkaTopicInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = options.Value.BootstrapServers
        }).Build();

        try
        {
            await adminClient.CreateTopicsAsync(
                [new TopicSpecification
                {
                    Name = BookingTopics.Confirmed,
                    NumPartitions = 3,
                    ReplicationFactor = 1
                }],
                new CreateTopicsOptions
                {
                    OperationTimeout = TimeSpan.FromSeconds(10),
                    RequestTimeout = TimeSpan.FromSeconds(10)
                });

            logger.LogInformation("Kafka topic {Topic} was created", BookingTopics.Confirmed);
        }
        catch (CreateTopicsException exception) when (
            exception.Results.Count > 0 &&
            exception.Results.All(result => result.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            logger.LogInformation("Kafka topic {Topic} already exists", BookingTopics.Confirmed);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not ensure that Kafka topic {Topic} exists", BookingTopics.Confirmed);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
