using Intentum.AI;
using Intentum.Core.Contracts;
using Intentum.McpServer.McpTools;

var builder = WebApplication.CreateBuilder(args);

var deepseekKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");
if (!string.IsNullOrEmpty(deepseekKey))
{
    builder.Services.AddIntentumDeepSeek(
        Intentum.AI.DeepSeek.DeepSeekOptions.FromEnvironment());
}
else
{
    builder.Services.AddSingleton<IIntentModel>(
        new Intentum.AI.Mock.MockIntentModel());
}

builder.Services.AddSingleton<InferIntentTool>();
builder.Services.AddSingleton<EvaluatePolicyTool>();

var app = builder.Build();

app.MapPost("/mcp/infer", (InferIntentTool tool, InferIntentTool.InferRequest[] events) =>
{
    var result = tool.Execute(events);
    return Results.Ok(result);
});

app.MapPost("/mcp/evaluate", (EvaluatePolicyTool tool, EvaluatePolicyTool.EvaluateRequest request) =>
{
    var result = tool.Execute(request);
    return Results.Ok(result);
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy", version = "1.1.10" }));

app.Run();
