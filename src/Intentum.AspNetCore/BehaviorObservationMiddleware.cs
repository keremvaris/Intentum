using Intentum.Core.Behavior;
using Microsoft.AspNetCore.Http;

namespace Intentum.AspNetCore;

/// <summary>
/// Middleware that automatically observes HTTP request behaviors into a BehaviorSpace.
/// </summary>
public sealed class BehaviorObservationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly BehaviorSpace _behaviorSpace;
    private readonly BehaviorObservationOptions _options;

    public BehaviorObservationMiddleware(
        RequestDelegate next,
        BehaviorSpace behaviorSpace,
        BehaviorObservationOptions? options = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _behaviorSpace = behaviorSpace ?? throw new ArgumentNullException(nameof(behaviorSpace));
        _options = options ?? new BehaviorObservationOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_options.Enabled)
        {
            var actor = _options.GetActor(context);
            var action = _options.GetAction(context);

            var metadata = new Dictionary<string, object>
            {
                { "path", context.Request.Path.Value ?? "" },
                { "method", context.Request.Method },
                { "statusCode", context.Response.StatusCode }
            };

            if (_options.IncludeHeaders && context.Request.Headers.Any())
            {
                metadata["headers"] = context.Request.Headers
                    .ToDictionary(h => h.Key, h => (object)h.Value.ToString());
            }

            _behaviorSpace.Observe(new BehaviorEvent(
                actor,
                action,
                DateTimeOffset.UtcNow,
                metadata));
        }

        await _next(context);
    }
}

/// <summary>
/// Options for behavior observation middleware.
/// </summary>
public sealed class BehaviorObservationOptions
{
    /// <summary>Whether observation is enabled. Defaults to true.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Whether to include request headers in metadata. Defaults to false.</summary>
    public bool IncludeHeaders { get; set; } = false;

    /// <summary>Function to extract actor from HTTP context. Defaults to "http".</summary>
    public Func<HttpContext, string> GetActor { get; set; } = _ => "http";

    /// <summary>Function to extract action from HTTP context. Defaults to method + path.</summary>
    public Func<HttpContext, string> GetAction { get; set; } = ctx =>
        $"{ctx.Request.Method.ToLowerInvariant()}_{ctx.Request.Path.Value?.Replace("/", "_").TrimStart('_') ?? "root"}";
}
