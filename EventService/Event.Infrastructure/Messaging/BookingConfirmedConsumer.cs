using System.Text.Json;
using Confluent.Kafka;
using Messaging.Contracts.Bookings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Event.Infrastructure.Messaging;

public sealed class BookingConfirmedConsumer(
    IOptions<KafkaOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<BookingConfirmedConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
    private readonly ConsumerConfig _consumerConfig = new()
    {
        BootstrapServers = options.Value.BootstrapServers,
        GroupId = options.Value.ConsumerGroup,
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,
        EnableAutoOffsetStore = false
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
        consumer.Subscribe(BookingTopics.Confirmed);

        logger.LogInformation(
            "Kafka consumer group {ConsumerGroup} subscribed to {Topic}",
            _consumerConfig.GroupId,
            BookingTopics.Confirmed);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> consumeResult;
                try
                {
                    consumeResult = await Task.Run(() => consumer.Consume(stoppingToken), stoppingToken);
                }
                catch (ConsumeException exception)
                {
                    logger.LogError(exception, "Kafka consume failed: {Reason}", exception.Error.Reason);
                    await Task.Delay(RetryDelay, stoppingToken);
                    continue;
                }

                try
                {
                    await ProcessMessageAsync(consumer, consumeResult, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Failed to process message at {TopicPartitionOffset}; the message will be retried",
                        consumeResult.TopicPartitionOffset);

                    try
                    {
                        consumer.Seek(consumeResult.TopicPartitionOffset);
                    }
                    catch (KafkaException seekException)
                    {
                        logger.LogWarning(
                            seekException,
                            "Could not seek to {TopicPartitionOffset}; Kafka will resume from the committed offset after reassignment",
                            consumeResult.TopicPartitionOffset);
                    }

                    await Task.Delay(RetryDelay, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(
        IConsumer<string, string> consumer,
        ConsumeResult<string, string> consumeResult,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(consumeResult.Message.Value))
        {
            logger.LogWarning(
                "Skipping empty message at {TopicPartitionOffset}",
                consumeResult.TopicPartitionOffset);
            consumer.Commit(consumeResult);
            return;
        }

        BookingConfirmed? message;
        try
        {
            message = JsonSerializer.Deserialize<BookingConfirmed>(
                consumeResult.Message.Value,
                SerializerOptions);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(
                exception,
                "Skipping malformed message at {TopicPartitionOffset}",
                consumeResult.TopicPartitionOffset);
            consumer.Commit(consumeResult);
            return;
        }

        if (message is null)
        {
            logger.LogWarning(
                "Skipping empty message at {TopicPartitionOffset}",
                consumeResult.TopicPartitionOffset);
            consumer.Commit(consumeResult);
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<BookingConfirmedProcessor>();
        var result = await processor.ProcessAsync(message, cancellationToken);

        LogProcessingResult(message, result);
        consumer.Commit(consumeResult);
    }

    private void LogProcessingResult(BookingConfirmed message, BookingConfirmedProcessingResult result)
    {
        switch (result)
        {
            case BookingConfirmedProcessingResult.Processed:
                logger.LogInformation(
                    "Booking {BookingId} reserved {SeatCount} seats for event {EventId}",
                    message.BookingId,
                    message.SeatCount,
                    message.EventId);
                break;
            case BookingConfirmedProcessingResult.Duplicate:
                logger.LogInformation("Booking {BookingId} was already processed", message.BookingId);
                break;
            case BookingConfirmedProcessingResult.EventNotFound:
                logger.LogWarning(
                    "Skipping booking {BookingId}: event {EventId} was not found",
                    message.BookingId,
                    message.EventId);
                break;
            case BookingConfirmedProcessingResult.NoAvailableSeats:
                logger.LogWarning(
                    "Skipping booking {BookingId}: event {EventId} has fewer than {SeatCount} available seats",
                    message.BookingId,
                    message.EventId,
                    message.SeatCount);
                break;
            case BookingConfirmedProcessingResult.InvalidSeatCount:
                logger.LogWarning(
                    "Skipping booking {BookingId}: seat count {SeatCount} is invalid",
                    message.BookingId,
                    message.SeatCount);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }
}
