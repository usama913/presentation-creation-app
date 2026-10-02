using System.Text;

namespace Gamma.Rendering.OpenSource.Pdf;

/// <summary>
/// Restricts arbitrary (possibly AI-generated) text down to what the
/// non-embedded Helvetica core font can render safely: printable ASCII
/// plus a small set of transliterated punctuation. This keeps the PDF
/// byte-writer simple (no font embedding, no full Unicode handling) while
/// still degrading gracefully instead of corrupting the file.
/// </summary>
internal static class PdfTextSanitizer
{
    /// <summary>Middle dot (WinAnsi 0xB7) used as the bullet glyph.</summary>
    public const char BulletChar = '·';

    public static string Sanitize(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            sb.Append(Transliterate(c));
        }

        return sb.ToString();
    }

    private static char Transliterate(char c) => c switch
    {
        '‘' or '’' or 'ʼ' => '\'',
        '“' or '”' => '"',
        '–' or '—' => '-',
        '•' or '●' or '◦' => BulletChar,
        _ when c is >= (char)32 and <= (char)126 => c,
        _ when c == '\t' => ' ',
        _ => '?'
    };

    /// <summary>Escapes a sanitized string for use inside a PDF literal string, i.e. "(...)".</summary>
    public static string EscapeForLiteral(string sanitized)
    {
        if (sanitized.Length == 0)
        {
            return sanitized;
        }

        var sb = new StringBuilder(sanitized.Length + 4);
        foreach (var c in sanitized)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '(': sb.Append("\\("); break;
                case ')': sb.Append("\\)"); break;
                default: sb.Append(c); break;
            }
        }

        return sb.ToString();
    }
}
