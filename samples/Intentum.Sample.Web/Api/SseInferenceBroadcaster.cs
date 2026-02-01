using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Intentum.Sample.Web.Api;

/// <summary>
/// Broadcasts inference events to SSE clients (e.g. fraud simulation).
/// </summary>
public sealed class SseInferenceBroadcaster
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly ConcurrentDictionary<Guid, Channel<byte[]>> _clients = new();

    /// <summary>
    /// Pushes an inference event to all connected SSE clients.
    /// </summary>
    public void Broadcast(object inferenceEvent)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(inferenceEvent, JsonOptions);
        var line = "data: " + Encoding.UTF8.GetString(payload) + "\n\n";
        var bytes = Encoding.UTF8.GetBytes(line);
        foreach (var ch in _clients.Values)
            ch.Writer.TryWrite(bytes);
    }

    /// <summary>
    /// Subscribes a new SSE client; returns a stream of "data: ...\n\n" lines.
    /// </summary>
    public async IAsyncEnumerable<byte[]> SubscribeAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        _clients[id] = channel;
        try
        {
            await foreach (var data in channel.Reader.ReadAllAsync(cancellationToken))
                yield return data;
        }
        finally
        {
            _clients.TryRemove(id, out _);
        }
    }
}
