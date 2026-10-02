using System.Globalization;

namespace Gamma.Rendering.OpenSource.Pdf;

/// <summary>Word-wrapping and simple color/text helpers shared by the slide-to-page layout.</summary>
internal static class PdfLayout
{
    /// <summary>Greedily wraps sanitized text to fit within <paramref name="maxWidthPt"/>, measuring with the core-font metrics.</summary>
    public static List<string> WrapText(string sanitizedText, double fontSize, bool bold, double maxWidthPt)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(sanitizedText))
        {
            lines.Add(string.Empty);
            return lines;
        }

        var words = sanitizedText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            lines.Add(string.Empty);
            return lines;
        }

        var current = string.Empty;
        foreach (var word in words)
        {
            var candidate = current.Length == 0 ? word : current + " " + word;
            if (HelveticaMetrics.MeasureWidthPt(candidate, fontSize, bold) <= maxWidthPt || current.Length == 0)
            {
                current = candidate;
            }
            else
            {
                lines.Add(current);
                current = word;
            }
        }

        if (current.Length > 0)
        {
            lines.Add(current);
        }

        return lines;
    }

    /// <summary>Converts a "RRGGBB" hex color into a PDF "r g b rg" nonstroking-color operator line.</summary>
    public static string RgFillOperator(string hex)
    {
        var (r, g, b) = ParseHex(hex);
        return string.Create(CultureInfo.InvariantCulture, $"{r:0.###} {g:0.###} {b:0.###} rg\n");
    }

    private static (double r, double g, double b) ParseHex(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        {
            return (0, 0, 0);
        }

        var r = (value >> 16) & 0xFF;
        var g = (value >> 8) & 0xFF;
        var b = value & 0xFF;
        return (r / 255.0, g / 255.0, b / 255.0);
    }

    public static string FormatPt(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
