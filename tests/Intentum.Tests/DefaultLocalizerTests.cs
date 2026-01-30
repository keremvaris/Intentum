using Intentum.Runtime.Localization;

namespace Intentum.Tests;

public sealed class DefaultLocalizerTests
{
    [Fact]
    public void DefaultLocalizer_DefaultConstructor_UsesEnglish()
    {
        var localizer = new DefaultLocalizer();
        Assert.Equal("Allow", localizer.Get(LocalizationKeys.DecisionAllow));
        Assert.Equal("Observe", localizer.Get(LocalizationKeys.DecisionObserve));
    }

    [Fact]
    public void DefaultLocalizer_English_ReturnsAllKeys()
    {
        var localizer = new DefaultLocalizer();
        Assert.Equal("Allow", localizer.Get(LocalizationKeys.DecisionAllow));
        Assert.Equal("Observe", localizer.Get(LocalizationKeys.DecisionObserve));
        Assert.Equal("Warn", localizer.Get(LocalizationKeys.DecisionWarn));
        Assert.Equal("Block", localizer.Get(LocalizationKeys.DecisionBlock));
        Assert.Equal("Escalate", localizer.Get(LocalizationKeys.DecisionEscalate));
        Assert.Equal("Require Authentication", localizer.Get(LocalizationKeys.DecisionRequireAuth));
        Assert.Equal("Rate Limit", localizer.Get(LocalizationKeys.DecisionRateLimit));
    }

    [Fact]
    public void DefaultLocalizer_Turkish_ReturnsAllKeys()
    {
        var localizer = new DefaultLocalizer("tr");
        Assert.Equal("İzin Ver", localizer.Get(LocalizationKeys.DecisionAllow));
        Assert.Equal("İzle", localizer.Get(LocalizationKeys.DecisionObserve));
        Assert.Equal("Uyar", localizer.Get(LocalizationKeys.DecisionWarn));
        Assert.Equal("Engelle", localizer.Get(LocalizationKeys.DecisionBlock));
        Assert.Equal("Yükselt", localizer.Get(LocalizationKeys.DecisionEscalate));
        Assert.Equal("Kimlik Doğrulama Gerekli", localizer.Get(LocalizationKeys.DecisionRequireAuth));
        Assert.Equal("Hız Sınırı", localizer.Get(LocalizationKeys.DecisionRateLimit));
    }

    [Fact]
    public void DefaultLocalizer_UnknownKey_ReturnsKey()
    {
        var localizer = new DefaultLocalizer();
        var unknown = "Unknown.Key";
        Assert.Equal(unknown, localizer.Get(unknown));
    }

    [Fact]
    public void DefaultLocalizer_UnknownLanguage_DefaultsToEnglish()
    {
        var localizer = new DefaultLocalizer("fr");
        Assert.Equal("Allow", localizer.Get(LocalizationKeys.DecisionAllow));
    }
}
