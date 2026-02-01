using Intentum.Core;
using Intentum.Core.Behavior;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Intentum.Sample.Blazor.Features.GreenwashingDetection;

/// <summary>
/// Görselden basit "yeşillik" skoru hesaplar; yüksek skor niyet pipeline'ına imagery sinyali ekler (demo).
/// </summary>
public static class GreenwashingImageAnalyzer
{
    private const double GreenDominanceThreshold = 0.38;

    /// <summary>
    /// Base64 görseli işler: yeşillik skoru (0–1) döner. Skor eşiği aşılırsa space'e imagery sinyali eklenir.
    /// </summary>
    public static (double GreenScore, bool AddedImagerySignal) AnalyzeAndAugment(string? imageBase64, BehaviorSpace space)
    {
        if (string.IsNullOrWhiteSpace(imageBase64))
            return (0, false);

        try
        {
            var bytes = Convert.FromBase64String(imageBase64.Trim());
            using var ms = new MemoryStream(bytes);
            using var image = Image.Load<Rgba32>(ms);
            var score = ComputeGreenScore(image);
            if (score >= GreenDominanceThreshold)
            {
                space.Observe("imagery", "nature.without.data");
                return (score, true);
            }
            return (score, false);
        }
        catch
        {
            return (0, false);
        }
    }

    /// <summary>
    /// Ortalama yeşil kanal / (R+G+B) oranı; 0–1 aralığına normalize.
    /// </summary>
    public static double ComputeGreenScore(Image<Rgba32> image)
    {
        if (image.Width == 0 || image.Height == 0)
            return 0;

        double sumR = 0, sumG = 0, sumB = 0;
        int count = 0;
        const int maxPixels = 10_000;
        image.ProcessPixelRows(accessor =>
        {
            var stepY = Math.Max(1, accessor.Height / 100);
            for (int y = 0; y < accessor.Height && count < maxPixels; y += stepY)
            {
                var row = accessor.GetRowSpan(y);
                var stepX = Math.Max(1, row.Length / 100);
                for (int x = 0; x < row.Length && count < maxPixels; x += stepX)
                {
                    var p = row[x];
                    sumR += p.R;
                    sumG += p.G;
                    sumB += p.B;
                    count++;
                }
            }
        });

        if (count == 0)
            return 0;

        var total = sumR + sumG + sumB;
        if (total <= 0)
            return 0;

        return Math.Min(1.0, sumG / total * 3); // *3 so that pure green -> ~1
    }
}
