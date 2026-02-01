using System.Runtime.CompilerServices;
using System.Text.Json;
using Confluent.Kafka;
using Intentum.Core.Behavior;
using Intentum.Core.Streaming;

namespace Intentum.Streaming.Kafka;

/// <summary>
/// Consumes behavior events from a Kafka topic and yields batches (topic → intent mapping).
/// Supports retry on consume errors and configurable message parsing (JSON or raw).
/// </summary>
public sealed class KafkaBehaviorStreamConsumer : IBehaviorStreamConsumer
{
    private readonly KafkaBehaviorStreamConsumerOptions _options;

    /// <summary>
    /// Creates a Kafka-backed behavior stream consumer.
    /// </summary>
    public KafkaBehaviorStreamConsumer(KafkaBehaviorStreamConsumerOptions? options = null)
    {
        _options = options ?? new KafkaBehaviorStreamConsumerOptions();
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<BehaviorEventBatch> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_options.Topic);

        var retries = 0;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string>? cr;
                try
                {
                    cr = consumer.Consume(cancellationToken);
                }
                catch (ConsumeException) when (retries < _options.RetryCount)
                {
                    retries++;
                    await Task.Delay(_options.RetryDelay, cancellationToken).ConfigureAwait(false);
                    continue;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (cr?.Message?.Value == null)
                    continue;

                retries = 0;
                var evt = ParseMessage(cr.Message.Value);
                yield return new BehaviorEventBatch([evt]);
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private BehaviorEvent ParseMessage(string value)
    {
        if (_options.ValueIsJson)
        {
            try
            {
                var doc = JsonDocument.Parse(value);
                var root = doc.RootElement;
                var actor = root.TryGetProperty("actor", out var a) ? a.GetString() ?? "kafka" : "kafka";
                var action = root.TryGetProperty("action", out var ac) ? ac.GetString() ?? "unknown" : "unknown";
                var occurredAt = root.TryGetProperty("occurredAt", out var o)
                    ? DateTimeOffset.TryParse(o.GetString(), out var dt) ? dt : DateTimeOffset.UtcNow
                    : DateTimeOffset.UtcNow;
                return new BehaviorEvent(actor, action, occurredAt);
            }
            catch (JsonException)
            {
                return new BehaviorEvent("kafka", value.Trim(), DateTimeOffset.UtcNow);
            }
        }

        return new BehaviorEvent("kafka", value.Trim(), DateTimeOffset.UtcNow);
    }
}
