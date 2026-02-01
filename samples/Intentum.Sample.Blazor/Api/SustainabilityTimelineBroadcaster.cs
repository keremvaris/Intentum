using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Broadcasts sustainability timeline events to SSE clients.
/// </summary>
public sealed class SustainabilityTimelineBroadcaster
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly ConcurrentDictionary<Guid, Channel<byte[]>> _clients = new();

    public void Broadcast(object sustainabilityEvent)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(sustainabilityEvent, JsonOptions);
        var line = "data: " + Encoding.UTF8.GetString(payload) + "\n\n";
        var bytes = Encoding.UTF8.GetBytes(line);
        foreach (var ch in _clients.Values)
            ch.Writer.TryWrite(bytes);
    }

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
