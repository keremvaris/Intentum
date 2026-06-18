namespace Intentum.AI.TokenCost;

public interface ITokenCounter
{
    int Count(string text);
}

public sealed class SimpleTokenCounter : ITokenCounter
{
    public int Count(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
