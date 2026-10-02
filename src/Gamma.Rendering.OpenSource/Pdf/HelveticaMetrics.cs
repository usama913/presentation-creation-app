namespace Gamma.Rendering.OpenSource.Pdf;

/// <summary>
/// Standard AFM character widths (in 1/1000 em) for the built-in Helvetica
/// and Helvetica-Bold core fonts, covering printable ASCII 32-126. These
/// are the same public metric tables every PDF-writing library ships,
/// since Helvetica is one of the 14 standard fonts every PDF viewer must
/// render without any embedded font data.
/// </summary>
internal static class HelveticaMetrics
{
    private const int FallbackWidth = 556;

    // Index 0 => char 32 (space) .. index 94 => char 126 ('~').
    private static readonly int[] Regular =
    {
        278, 278, 355, 556, 556, 889, 667, 191, 333, 333, 389, 584, 278, 333, 278, 278, // 32-47
        556, 556, 556, 556, 556, 556, 556, 556, 556, 556, 278, 278, 584, 584, 584, 556, // 48-63
        1015, 667, 667, 722, 722, 667, 611, 778, 722, 278, 500, 667, 556, 833, 722, 778, // 64-79
        667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 278, 278, 278, 469, 556, // 80-95
        333, 556, 556, 500, 556, 556, 278, 556, 556, 222, 222, 500, 222, 833, 556, 556, // 96-111
        556, 556, 333, 500, 278, 556, 500, 722, 500, 500, 500, 334, 260, 334, 584        // 112-126
    };

    private static readonly int[] Bold =
    {
        278, 333, 474, 556, 556, 889, 722, 238, 333, 333, 389, 584, 278, 333, 278, 278,
        556, 556, 556, 556, 556, 556, 556, 556, 556, 556, 333, 333, 584, 584, 584, 611,
        975, 722, 722, 722, 722, 667, 611, 778, 722, 278, 556, 722, 611, 833, 722, 778,
        667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 333, 278, 333, 584, 556,
        333, 556, 611, 556, 611, 556, 333, 611, 611, 278, 278, 556, 278, 889, 611, 611,
        611, 611, 389, 556, 333, 611, 556, 778, 556, 556, 500, 389, 280, 389, 584
    };

    /// <summary>Width of a single character, in 1/1000 em, for the given font weight.</summary>
    public static int CharWidth(char c, bool bold)
    {
        if (c == PdfTextSanitizer.BulletChar)
        {
            return 333;
        }

        if (c < 32 || c > 126)
        {
            return FallbackWidth;
        }

        var table = bold ? Bold : Regular;
        return table[c - 32];
    }

    /// <summary>Total width of a string in points, for the given font size.</summary>
    public static double MeasureWidthPt(string text, double fontSize, bool bold)
    {
        var thousandths = 0;
        foreach (var c in text)
        {
            thousandths += CharWidth(c, bold);
        }

        return thousandths / 1000.0 * fontSize;
    }
}
