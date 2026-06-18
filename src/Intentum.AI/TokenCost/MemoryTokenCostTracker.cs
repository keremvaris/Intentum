namespace Intentum.AI.TokenCost;

public sealed class MemoryTokenCostTracker : ITokenCostTracker
{
    private readonly List<TokenCost> _costs = new();
    private readonly object _lock = new();

    public void Track(TokenCost cost)
    {
        lock (_lock)
            _costs.Add(cost);
    }

    public TokenCost GetTotal(string? model = null)
    {
        lock (_lock)
        {
            var filtered = model != null
                ? _costs.Where(c => c.Model == model).ToList()
                : _costs;

            return new TokenCost(
                model ?? "*",
                filtered.Sum(c => c.PromptTokens),
                filtered.Sum(c => c.CompletionTokens),
                filtered.Sum(c => c.Cost));
        }
    }

    public void Reset()
    {
        lock (_lock)
            _costs.Clear();
    }
}
