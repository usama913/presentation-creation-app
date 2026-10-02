using System.Text;
using Gamma.Core.Models;

namespace Gamma.Rendering.OpenSource.Pdf;

/// <summary>Builds the PDF content-stream operators for a single slide/page, per layout.</summary>
internal static class PdfSlideContentBuilder
{
    public const double PageWidth = 960;   // 13.333in @ 72pt/in — matches the PPTX renderer's 16:9 aspect.
    public const double PageHeight = 540;  // 7.5in
    private const double Margin = 54;      // 0.75in
    public const double ContentWidth = PageWidth - (2 * Margin);

    private const string FontRegular = "F1";
    private const string FontBold = "F2";
    private const string FontItalic = "F3";

    public static string Build(Slide slide, ThemeOptions theme, string? deckSubtitleForFirstSlide)
    {
        return slide.Layout switch
        {
            SlideLayout.TitleSlide => BuildTitleSlide(slide, theme, deckSubtitleForFirstSlide),
            SlideLayout.SectionHeader => BuildSectionHeader(slide, theme),
            SlideLayout.Quote => BuildQuote(slide, theme),
            SlideLayout.Closing => BuildClosing(slide, theme),
            _ => BuildTitleAndBullets(slide, theme)
        };
    }

    private static string BuildTitleAndBullets(Slide slide, ThemeOptions theme)
    {
        var sb = new StringBuilder();
        const double titleSize = 24;
        var titleLines = PdfLayout.WrapText(PdfTextSanitizer.Sanitize(slide.Title), titleSize, bold: true, ContentWidth);

        sb.Append(PdfLayout.RgFillOperator(theme.AccentColorHex));
        var y = PageHeight - Margin - titleSize;
        foreach (var line in titleLines)
        {
            AppendLine(sb, FontBold, titleSize, Margin, y, line);
            y -= titleSize * 1.25;
        }

        const double bodySize = 15;
        const double lineHeight = bodySize * 1.35;
        const double hangingIndent = 16;
        sb.Append(PdfLayout.RgFillOperator(theme.PrimaryColorHex));

        y -= 20; // gap between title block and body
        foreach (var bullet in slide.Bullets)
        {
            var wrapped = PdfLayout.WrapText(PdfTextSanitizer.Sanitize(bullet), bodySize, bold: false, ContentWidth - hangingIndent);
            for (var i = 0; i < wrapped.Count; i++)
            {
                var text = i == 0 ? $"{PdfTextSanitizer.BulletChar}  {wrapped[i]}" : wrapped[i];
                var x = i == 0 ? Margin : Margin + hangingIndent;
                AppendLine(sb, FontRegular, bodySize, x, y, text);
                y -= lineHeight;
            }
        }

        return sb.ToString();
    }

    private static string BuildTitleSlide(Slide slide, ThemeOptions theme, string? subtitle)
    {
        var sb = new StringBuilder();
        const double titleSize = 34;
        var titleLines = PdfLayout.WrapText(PdfTextSanitizer.Sanitize(slide.Title), titleSize, bold: true, ContentWidth);

        sb.Append(PdfLayout.RgFillOperator(theme.AccentColorHex));
        var startY = PageHeight / 2 + (titleLines.Count * titleSize * 0.65);
        var y = startY;
        foreach (var line in titleLines)
        {
            AppendCenteredLine(sb, FontBold, titleSize, bold: true, y, line);
            y -= titleSize * 1.25;
        }

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            const double subtitleSize = 16;
            sb.Append(PdfLayout.RgFillOperator(theme.PrimaryColorHex));
            y -= 20;
            foreach (var line in PdfLayout.WrapText(PdfTextSanitizer.Sanitize(subtitle), subtitleSize, bold: false, ContentWidth))
            {
                AppendCenteredLine(sb, FontRegular, subtitleSize, bold: false, y, line);
                y -= subtitleSize * 1.35;
            }
        }

        return sb.ToString();
    }

    private static string BuildSectionHeader(Slide slide, ThemeOptions theme)
    {
        var sb = new StringBuilder();
        const double titleSize = 30;
        var lines = PdfLayout.WrapText(PdfTextSanitizer.Sanitize(slide.Title), titleSize, bold: true, ContentWidth);

        sb.Append(PdfLayout.RgFillOperator(theme.AccentColorHex));
        var y = PageHeight / 2 + (lines.Count * titleSize * 0.6);
        foreach (var line in lines)
        {
            AppendCenteredLine(sb, FontBold, titleSize, bold: true, y, line);
            y -= titleSize * 1.25;
        }

        return sb.ToString();
    }

    private static string BuildQuote(Slide slide, ThemeOptions theme)
    {
        var sb = new StringBuilder();
        const double bodySize = 20;
        var text = string.IsNullOrWhiteSpace(slide.Body) ? slide.Title : slide.Body;
        var lines = PdfLayout.WrapText(PdfTextSanitizer.Sanitize(text), bodySize, bold: false, ContentWidth - 108);

        sb.Append(PdfLayout.RgFillOperator(theme.PrimaryColorHex));
        var y = PageHeight / 2 + (lines.Count * bodySize * 0.65);
        foreach (var line in lines)
        {
            AppendCenteredLine(sb, FontItalic, bodySize, bold: false, y, line);
            y -= bodySize * 1.35;
        }

        return sb.ToString();
    }

    private static string BuildClosing(Slide slide, ThemeOptions theme)
    {
        var sb = new StringBuilder();
        const double titleSize = 28;
        var titleLines = PdfLayout.WrapText(PdfTextSanitizer.Sanitize(slide.Title), titleSize, bold: true, ContentWidth);

        sb.Append(PdfLayout.RgFillOperator(theme.AccentColorHex));
        var y = PageHeight / 2 + 40 + (titleLines.Count * titleSize * 0.3);
        foreach (var line in titleLines)
        {
            AppendCenteredLine(sb, FontBold, titleSize, bold: true, y, line);
            y -= titleSize * 1.25;
        }

        if (!string.IsNullOrWhiteSpace(slide.Body))
        {
            const double bodySize = 16;
            sb.Append(PdfLayout.RgFillOperator(theme.PrimaryColorHex));
            y -= 20;
            foreach (var line in PdfLayout.WrapText(PdfTextSanitizer.Sanitize(slide.Body), bodySize, bold: false, ContentWidth - 60))
            {
                AppendCenteredLine(sb, FontRegular, bodySize, bold: false, y, line);
                y -= bodySize * 1.35;
            }
        }

        return sb.ToString();
    }

    private static void AppendLine(StringBuilder sb, string fontResource, double fontSize, double x, double y, string sanitizedText)
    {
        sb.Append("BT\n/").Append(fontResource).Append(' ').Append(PdfLayout.FormatPt(fontSize)).Append(" Tf\n");
        sb.Append(PdfLayout.FormatPt(x)).Append(' ').Append(PdfLayout.FormatPt(y)).Append(" Td\n");
        sb.Append('(').Append(PdfTextSanitizer.EscapeForLiteral(sanitizedText)).Append(") Tj\nET\n");
    }

    private static void AppendCenteredLine(StringBuilder sb, string fontResource, double fontSize, bool bold, double y, string sanitizedText)
    {
        var width = HelveticaMetrics.MeasureWidthPt(sanitizedText, fontSize, bold);
        var x = (PageWidth - width) / 2;
        AppendLine(sb, fontResource, fontSize, x, y, sanitizedText);
    }
}
