using System.Text.Json;
using Booking.Application.Common.Interfaces;
using Confluent.Kafka;
using Messaging.Contracts.Bookings;
using Microsoft.Extensions.Options;

namespace Booking.Infrastructure.Messaging;

public sealed class KafkaBookingConfirmedPublisher : IBookingConfirmedPublisher, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IProducer<string, string> _producer;

    public KafkaBookingConfirmedPublisher(IOptions<KafkaOptions> options)
    {
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers
        }).Build();
    }

    public async Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken = default)
    {
        var kafkaMessage = new Message<string, string>
        {
            Key = message.EventId.ToString(),
            Value = JsonSerializer.Serialize(message, SerializerOptions)
        };

        await _producer.ProduceAsync(BookingTopics.Confirmed, kafkaMessage, cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}

