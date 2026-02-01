namespace Intentum.Streaming.Kafka;

/// <summary>
/// Options for the Kafka behavior stream consumer: topic mapping, retry and error handling.
/// </summary>
public sealed class KafkaBehaviorStreamConsumerOptions
{
    /// <summary>Bootstrap servers (e.g. "localhost:9092").</summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>Topic to consume (topic → intent: messages are mapped to behavior events).</summary>
    public string Topic { get; set; } = "behavior-events";

    /// <summary>Consumer group id.</summary>
    public string GroupId { get; set; } = "intentum-consumer";

    /// <summary>Number of retries on consume error before failing. Default 3.</summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>Delay between retries. Default 1 second.</summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>If true, each Kafka message value is expected to be JSON: { "actor": "...", "action": "...", "occurredAt": "ISO8601" }. If false, raw string is treated as action with actor "kafka".</summary>
    public bool ValueIsJson { get; set; } = true;
}
