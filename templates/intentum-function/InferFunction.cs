using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Runtime;
using Intentum.Runtime.Policy;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Intentum.Function;

public class InferFunction
{
    private readonly IIntentModel _model;
    private readonly IntentPolicy _policy;
    private readonly ILogger _logger;

    public InferFunction(IIntentModel model, IntentPolicy policy, ILoggerFactory loggerFactory)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _logger = loggerFactory?.CreateLogger<InferFunction>() ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    [Function("InferIntent")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "intent/infer")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        InferRequest? body = null;
        try
        {
            body = await req.ReadFromJsonAsync<InferRequest>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid request body");
        }

        if (body?.Events is null || body.Events.Count == 0)
        {
            var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await bad.WriteAsJsonAsync(new { error = "Body must contain events array with actor/action." }, cancellationToken);
            return bad;
        }

        var space = new BehaviorSpace();
        foreach (var e in body.Events)
            space.Observe(new BehaviorEvent(e.Actor, e.Action, DateTimeOffset.UtcNow));
        var intent = _model.Infer(space);
        var decision = intent.Decide(_policy);

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            intent.Name,
            intent.Confidence.Level,
            intent.Confidence.Score,
            Decision = decision.ToString()
        }, cancellationToken);
        return response;
    }
}

internal record InferRequest(IReadOnlyList<EventDto> Events);
internal record EventDto(string Actor, string Action);
