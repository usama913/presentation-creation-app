using System.Globalization;
using Gamma.Core.Models;

namespace Gamma.Rendering.OpenSource.Pdf;

/// <summary>
/// Hand-assembles a valid PDF (one page per slide) directly from a
/// <see cref="PresentationDocument"/>, using nothing beyond plain string
/// building — no PDF library, no license, no network dependency. Text is
/// drawn with the built-in Helvetica core fonts, so nothing needs to be
/// embedded.
/// </summary>
internal static class MinimalPdfWriter
{
    public static byte[] Write(PresentationDocument document)
    {
        var slides = document.Slides.Count > 0
            ? document.Slides
            : new List<Slide> { new() { Title = document.Title, Layout = SlideLayout.TitleSlide } };

        var writer = new PdfDocumentWriter();

        // Reserve low object numbers for Catalog/Pages/Fonts before writing pages,
        // so every page's /Parent and /Resources references are known up front.
        var catalogNumber = writer.NextObjectNumber;      // 1
        var pagesNumber = catalogNumber + 1;               // 2
        var fontRegularNumber = pagesNumber + 1;           // 3
        var fontBoldNumber = fontRegularNumber + 1;        // 4
        var fontItalicNumber = fontBoldNumber + 1;         // 5
        var firstPageNumber = fontItalicNumber + 1;        // 6

        // Objects must be written in ascending numeric order for this writer's
        // offset bookkeeping, so placeholder-write Catalog/Pages last using the
        // numbers we already reserved logically — instead, write in true order:
        // Catalog and Pages need the page object numbers, which we only know
        // once we've laid out all slides. So compute page/content numbers
        // mathematically (2 objects per slide: page + content stream) instead
        // of writing Catalog/Pages first.
        var pageNumbers = new List<int>();
        var contentNumbers = new List<int>();
        for (var i = 0; i < slides.Count; i++)
        {
            pageNumbers.Add(firstPageNumber + (i * 2));
            contentNumbers.Add(firstPageNumber + (i * 2) + 1);
        }

        // Now write objects in the exact numeric order reserved above.
        WriteCatalog(writer, pagesNumber);
        WritePages(writer, pageNumbers);
        WriteFont(writer, "Helvetica");
        WriteFont(writer, "Helvetica-Bold");
        WriteFont(writer, "Helvetica-Oblique");

        for (var i = 0; i < slides.Count; i++)
        {
            var subtitle = i == 0 ? document.Subtitle : null;
            var content = PdfSlideContentBuilder.Build(slides[i], document.Theme, subtitle);

            WritePage(writer, pagesNumber, fontRegularNumber, fontBoldNumber, fontItalicNumber, contentNumbers[i]);
            WriteContentStream(writer, content);
        }

        return writer.Finish(catalogNumber);
    }

    private static void WriteCatalog(PdfDocumentWriter writer, int pagesNumber) =>
        writer.WriteDictionaryObject(string.Create(CultureInfo.InvariantCulture, $"/Type /Catalog /Pages {pagesNumber} 0 R"));

    private static void WritePages(PdfDocumentWriter writer, List<int> pageNumbers)
    {
        var kids = string.Join(' ', pageNumbers.Select(n => $"{n} 0 R"));
        writer.WriteDictionaryObject(string.Create(CultureInfo.InvariantCulture, $"/Type /Pages /Kids [{kids}] /Count {pageNumbers.Count}"));
    }

    private static void WriteFont(PdfDocumentWriter writer, string baseFont) =>
        writer.WriteDictionaryObject($"/Type /Font /Subtype /Type1 /BaseFont /{baseFont} /Encoding /WinAnsiEncoding");

    private static void WritePage(PdfDocumentWriter writer, int parent, int fontRegular, int fontBold, int fontItalic, int contentNumber)
    {
        var dict = string.Create(CultureInfo.InvariantCulture, $"""
             /Type /Page /Parent {parent} 0 R /MediaBox [0 0 {PdfSlideContentBuilder.PageWidth} {PdfSlideContentBuilder.PageHeight}] /Resources <</Font <</F1 {fontRegular} 0 R /F2 {fontBold} 0 R /F3 {fontItalic} 0 R>>>> /Contents {contentNumber} 0 R
            """);
        writer.WriteDictionaryObject(dict);
    }

    private static void WriteContentStream(PdfDocumentWriter writer, string content) =>
        writer.WriteStreamObject(string.Empty, content);
}
