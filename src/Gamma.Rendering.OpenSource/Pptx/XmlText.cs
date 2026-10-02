using System.Text;

namespace Gamma.Rendering.OpenSource.Pptx;

/// <summary>Minimal XML text/attribute escaping — no external XML library needed for this.</summary>
internal static class XmlText
{
    /// <summary>Escapes text for use inside an element body (e.g. inside &lt;a:t&gt;...&lt;/a:t&gt;).</summary>
    public static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            switch (c)
            {
                case '&': sb.Append("&amp;"); break;
                case '<': sb.Append("&lt;"); break;
                case '>': sb.Append("&gt;"); break;
                case '"': sb.Append("&quot;"); break;
                case '\'': sb.Append("&apos;"); break;
                // Strip control characters the XML 1.0 spec disallows (other than tab/CR/LF),
                // which some LLM output occasionally contains.
                case '\r': break;
                case '\n': sb.Append(' '); break;
                default:
                    if (c < 0x20 && c != '\t')
                    {
                        break;
                    }
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }
}
