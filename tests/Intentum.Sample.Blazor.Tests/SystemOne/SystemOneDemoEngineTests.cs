using System.Text.Json;
using Intentum.AI.SystemOne;
using Intentum.Sample.Blazor.Api;

namespace Intentum.Sample.Blazor.Tests.SystemOne;

public sealed class SystemOneDemoEngineTests
{
    private static readonly Dictionary<string, string?> Catalog = new()
    {
        ["Billing"] = "invoices, charges, refunds, payment problems",
        ["Technical"] = "bugs, outages, login failures, integration errors",
        ["Account"] = "profile, password reset, account recovery",
        ["Other"] = "everything else",
    };

    private static Dictionary<string, SystemOneQuestion> IntentQuestion() => new()
    {
        ["intent"] = SystemOneQuestion.Choice("Which intent?", Catalog),
    };

    private static JsonElement State(string action)
    {
        using var doc = JsonDocument.Parse($"{{\"events\":[\"user-1:{action}\"]}}");
        return doc.RootElement.Clone();
    }

    [Theory]
    [InlineData("refund.requested", "Billing")]
    [InlineData("payment.charged_twice", "Billing")]
    [InlineData("invoice.viewed", "Billing")]
    [InlineData("login.failed", "Technical")]
    [InlineData("error.500", "Technical")]
    [InlineData("bug.found", "Technical")]
    [InlineData("outage.started", "Technical")]
    [InlineData("password.reset", "Account")]
    [InlineData("profile.updated", "Account")]
    [InlineData("account.locked", "Account")]
    [InlineData("misc.action", "Other")]
    public void Decide_KeywordRoutesToExpectedIntent(string action, string expected)
    {
        var response = SystemOneDemoEngine.Decide(State(action), IntentQuestion());

        Assert.Equal(expected, response.Answers["intent"].Choice);
    }

    [Fact]
    public void Decide_SameInputTwice_ProducesIdenticalResult()
    {
        var state = State("payment.charged_twice");

        var first = SystemOneDemoEngine.Decide(state, IntentQuestion());
        var second = SystemOneDemoEngine.Decide(state, IntentQuestion());

        var a = first.Answers["intent"];
        var b = second.Answers["intent"];
        Assert.Equal(a.Choice, b.Choice);
        Assert.Equal(a.Confidence, b.Confidence);
        Assert.Equal(
            a.Probabilities!.OrderBy(p => p.Key).Select(p => (p.Key, p.Value)),
            b.Probabilities!.OrderBy(p => p.Key).Select(p => (p.Key, p.Value)));
    }

    [Fact]
    public void Decide_CoversEveryCatalogOption_WithProbabilitiesSummingToOne()
    {
        var response = SystemOneDemoEngine.Decide(State("refund.requested"), IntentQuestion());

        var probabilities = response.Answers["intent"].Probabilities!;
        Assert.Equal(Catalog.Keys.OrderBy(k => k), probabilities.Keys.OrderBy(k => k));
        Assert.Equal(1.0, probabilities.Values.Sum(), 3);
        Assert.Equal("Billing", response.Answers["intent"].Choice);
    }

    [Fact]
    public void Decide_StateAsPlainString_StillMatchesKeyword()
    {
        using var doc = JsonDocument.Parse("\"user clicked outage.started link\"");

        var response = SystemOneDemoEngine.Decide(doc.RootElement.Clone(), IntentQuestion());

        Assert.Equal("Technical", response.Answers["intent"].Choice);
    }

    [Fact]
    public void Decide_MultipleEventsOfSameGroup_AccumulatesAndWins()
    {
        using var doc = JsonDocument.Parse(
            "{\"events\":[\"user-1:refund.requested\",\"user-1:invoice.viewed\"]}");

        var response = SystemOneDemoEngine.Decide(doc.RootElement.Clone(), IntentQuestion());

        var answer = response.Answers["intent"];
        Assert.Equal("Billing", answer.Choice);
        Assert.Equal(Math.Min(0.97, 0.55 + 0.08 * 2), answer.Confidence);
    }

    [Fact]
    public void Decide_EqualCountsAcrossGroups_TieBreaksToFirstRule()
    {
        using var doc = JsonDocument.Parse(
            "{\"events\":[\"user-1:refund.requested\",\"user-1:login.failed\"]}");

        var response = SystemOneDemoEngine.Decide(doc.RootElement.Clone(), IntentQuestion());

        Assert.Equal("Billing", response.Answers["intent"].Choice);
    }

    [Fact]
    public void Decide_NoQuestions_ReturnsEmptyAnswers()
    {
        var response = SystemOneDemoEngine.Decide(State("refund.requested"), null);

        Assert.Empty(response.Answers);
    }
}
