namespace Intentum.AI.TokenCost;

public sealed record TokenCost(
    string Model,
    int PromptTokens,
    int CompletionTokens,
    decimal Cost);

public interface ITokenCostTracker
{
    void Track(TokenCost cost);
    TokenCost GetTotal(string? model = null);
    void Reset();
}
