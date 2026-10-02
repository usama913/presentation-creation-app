using Gamma.Core.Models;
using static Gamma.Rendering.OpenSource.Pptx.PptxShapeBuilder;

namespace Gamma.Rendering.OpenSource.Pptx;

/// <summary>Lays a single <see cref="Slide"/> out into a full &lt;p:sld&gt; XML document, per layout.</summary>
internal static class PptxSlideXmlBuilder
{
    // 16:9 slide is 12192000 x 6858000 EMU. Keep a consistent side margin.
    private const long SlideWidth = 12192000;
    private const long Margin = 685800; // 0.75in
    private const long ContentWidth = SlideWidth - (2 * Margin);

    public static string Build(Slide slide, ThemeOptions theme, string? deckSubtitleForFirstSlide)
    {
        var shapes = slide.Layout switch
        {
            SlideLayout.TitleSlide => BuildTitleSlide(slide, theme, deckSubtitleForFirstSlide),
            SlideLayout.SectionHeader => BuildSectionHeader(slide, theme),
            SlideLayout.Quote => BuildQuote(slide, theme),
            SlideLayout.Closing => BuildClosing(slide, theme),
            _ => BuildTitleAndBullets(slide, theme)
        };

        return $"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <p:sld xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main">
              <p:cSld>
                <p:spTree>
                  <p:nvGrpSpPr>
                    <p:cNvPr id="1" name=""/>
                    <p:cNvGrpSpPr/>
                    <p:nvPr/>
                  </p:nvGrpSpPr>
                  <p:grpSpPr/>
            {shapes}
                </p:spTree>
              </p:cSld>
              <p:clrMapOvr>
                <a:masterClrMapping/>
              </p:clrMapOvr>
            </p:sld>
            """;
    }

    private static string BuildTitleAndBullets(Slide slide, ThemeOptions theme)
    {
        var title = TextBox(
            id: 2, x: Margin, y: 365760, cx: ContentWidth, cy: 1000000,
            paragraphs: new[] { new Paragraph(new[] { new Run(slide.Title) }, Bulleted: false) },
            fontSizeHundredths: 3200, colorHex: theme.AccentColorHex, bold: true, fontFamily: theme.FontFamily);

        var bulletParagraphs = slide.Bullets.Count > 0
            ? slide.Bullets.Select(b => new Paragraph(new[] { new Run(b) }, Bulleted: true)).ToArray()
            : new[] { new Paragraph(Array.Empty<Run>(), Bulleted: false) };

        var body = TextBox(
            id: 3, x: Margin, y: 1500000, cx: ContentWidth, cy: 5000000,
            paragraphs: bulletParagraphs,
            fontSizeHundredths: 2000, colorHex: theme.PrimaryColorHex, bold: false, fontFamily: theme.FontFamily);

        return title + "\n" + body;
    }

    private static string BuildTitleSlide(Slide slide, ThemeOptions theme, string? subtitle)
    {
        var title = TextBox(
            id: 2, x: Margin, y: 2514600, cx: ContentWidth, cy: 1300000,
            paragraphs: new[] { new Paragraph(new[] { new Run(slide.Title) }, Bulleted: false, Align: "ctr") },
            fontSizeHundredths: 4400, colorHex: theme.AccentColorHex, bold: true, fontFamily: theme.FontFamily,
            verticalAnchor: "ctr");

        if (string.IsNullOrWhiteSpace(subtitle))
        {
            return title;
        }

        var subtitleBox = TextBox(
            id: 3, x: Margin + 914400, y: 3960000, cx: ContentWidth - 1828800, cy: 800000,
            paragraphs: new[] { new Paragraph(new[] { new Run(subtitle) }, Bulleted: false, Align: "ctr") },
            fontSizeHundredths: 2000, colorHex: theme.PrimaryColorHex, bold: false, fontFamily: theme.FontFamily,
            verticalAnchor: "ctr");

        return title + "\n" + subtitleBox;
    }

    private static string BuildSectionHeader(Slide slide, ThemeOptions theme)
    {
        return TextBox(
            id: 2, x: Margin, y: 2743200, cx: ContentWidth, cy: 1371600,
            paragraphs: new[] { new Paragraph(new[] { new Run(slide.Title) }, Bulleted: false, Align: "ctr") },
            fontSizeHundredths: 4000, colorHex: theme.AccentColorHex, bold: true, fontFamily: theme.FontFamily,
            verticalAnchor: "ctr");
    }

    private static string BuildQuote(Slide slide, ThemeOptions theme)
    {
        var text = string.IsNullOrWhiteSpace(slide.Body) ? slide.Title : slide.Body;

        var box = TextBox(
            id: 2, x: Margin + 685800, y: 2286000, cx: ContentWidth - 1371600, cy: 2286000,
            paragraphs: new[] { new Paragraph(new[] { new Run(text, Italic: true) }, Bulleted: false, Align: "ctr") },
            fontSizeHundredths: 2800, colorHex: theme.PrimaryColorHex, bold: false, fontFamily: theme.FontFamily,
            verticalAnchor: "ctr");

        return box;
    }

    private static string BuildClosing(Slide slide, ThemeOptions theme)
    {
        var title = TextBox(
            id: 2, x: Margin, y: 2286000, cx: ContentWidth, cy: 1000000,
            paragraphs: new[] { new Paragraph(new[] { new Run(slide.Title) }, Bulleted: false, Align: "ctr") },
            fontSizeHundredths: 3600, colorHex: theme.AccentColorHex, bold: true, fontFamily: theme.FontFamily,
            verticalAnchor: "ctr");

        if (string.IsNullOrWhiteSpace(slide.Body))
        {
            return title;
        }

        var body = TextBox(
            id: 3, x: Margin + 685800, y: 3505200, cx: ContentWidth - 1371600, cy: 1000000,
            paragraphs: new[] { new Paragraph(new[] { new Run(slide.Body) }, Bulleted: false, Align: "ctr") },
            fontSizeHundredths: 2200, colorHex: theme.PrimaryColorHex, bold: false, fontFamily: theme.FontFamily,
            verticalAnchor: "ctr");

        return title + "\n" + body;
    }
}
