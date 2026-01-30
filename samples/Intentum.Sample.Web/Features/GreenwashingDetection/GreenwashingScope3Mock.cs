using Intentum.Sample.Web.Api;

namespace Intentum.Sample.Web.Features.GreenwashingDetection;

/// <summary>
/// Sabit Scope 3 tedarikçi listesi (demo): 5 tedarikçi, 3 doğrulanmış.
/// </summary>
public static class GreenwashingScope3Mock
{
    private static readonly GreenwashingScope3Summary Instance = new(
        TotalSuppliers: 5,
        VerifiedCount: 3,
        Details:
        [
            new GreenwashingScope3Supplier("Tedarikçi A (hammadde)", true),
            new GreenwashingScope3Supplier("Tedarikçi B (lojistik)", true),
            new GreenwashingScope3Supplier("Tedarikçi C (üretim)", true),
            new GreenwashingScope3Supplier("Tedarikçi D (ambalaj)", false),
            new GreenwashingScope3Supplier("Tedarikçi E (atık)", false)
        ]);

    public static GreenwashingScope3Summary Get() => Instance;
}
