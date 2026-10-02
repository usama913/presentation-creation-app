using System.IO.Compression;
using System.Text;
using Gamma.Core.Models;

namespace Gamma.Rendering.OpenSource.Pptx;

/// <summary>
/// Hand-assembles a valid .pptx (an OOXML / Open Packaging Convention zip)
/// from a <see cref="PresentationDocument"/>, using nothing beyond
/// System.IO.Compression and plain string/XML building. No DocumentFormat.OpenXml
/// package, no license, no network dependency.
/// </summary>
internal static class MinimalPptxWriter
{
    public static byte[] Write(PresentationDocument document)
    {
        var slideCount = Math.Max(document.Slides.Count, 1);
        var slides = document.Slides.Count > 0
            ? document.Slides
            : new List<Slide> { new() { Title = document.Title, Layout = SlideLayout.TitleSlide } };

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteText(archive, "[Content_Types].xml", BuildContentTypes(slideCount));
            WriteText(archive, "_rels/.rels", OoxmlTemplates.RootRelsTemplate);

            var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            WriteText(archive, "docProps/core.xml", string.Format(OoxmlTemplates.CorePropsTemplate, XmlText.Escape(document.Title), now));
            WriteText(archive, "docProps/app.xml", string.Format(OoxmlTemplates.AppPropsTemplate, slideCount));

            WriteText(archive, "ppt/presentation.xml", BuildPresentationXml(slides.Count));
            WriteText(archive, "ppt/_rels/presentation.xml.rels", BuildPresentationRels(slides.Count));

            WriteText(archive, "ppt/slideMasters/slideMaster1.xml", OoxmlTemplates.SlideMasterTemplate);
            WriteText(archive, "ppt/slideMasters/_rels/slideMaster1.xml.rels", OoxmlTemplates.SlideMasterRelsTemplate);

            WriteText(archive, "ppt/slideLayouts/slideLayout1.xml", OoxmlTemplates.SlideLayoutTemplate);
            WriteText(archive, "ppt/slideLayouts/_rels/slideLayout1.xml.rels", OoxmlTemplates.SlideLayoutRelsTemplate);

            WriteText(archive, "ppt/theme/theme1.xml", string.Format(
                OoxmlTemplates.ThemeTemplate,
                document.Theme.PrimaryColorHex,
                document.Theme.AccentColorHex,
                document.Theme.BackgroundColorHex));

            for (var i = 0; i < slides.Count; i++)
            {
                var slideNumber = i + 1;
                var subtitle = i == 0 ? document.Subtitle : null;
                var slideXml = PptxSlideXmlBuilder.Build(slides[i], document.Theme, subtitle);
                WriteText(archive, $"ppt/slides/slide{slideNumber}.xml", slideXml);
                WriteText(archive, $"ppt/slides/_rels/slide{slideNumber}.xml.rels", BuildSlideRels());
            }
        }

        return memoryStream.ToArray();
    }

    private static string BuildContentTypes(int slideCount)
    {
        var overrides = new StringBuilder();
        for (var i = 1; i <= slideCount; i++)
        {
            overrides.Append(
                $"""  <Override PartName="/ppt/slides/slide{i}.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slide+xml"/>{"\n"}""");
        }

        return string.Format(OoxmlTemplates.ContentTypesTemplate, overrides.ToString().TrimEnd('\n'));
    }

    private static string BuildPresentationXml(int slideCount)
    {
        var sldIdLst = new StringBuilder();
        for (var i = 0; i < slideCount; i++)
        {
            var slideId = 256 + i;
            var rId = i + 1; // rId1..rIdN reserved for slides in presentation.xml.rels
            sldIdLst.Append($"""    <p:sldId id="{slideId}" r:id="rId{rId}"/>{"\n"}""");
        }

        return string.Format(OoxmlTemplates.PresentationTemplate, sldIdLst.ToString().TrimEnd('\n'));
    }

    private static string BuildPresentationRels(int slideCount)
    {
        var rels = new StringBuilder();
        for (var i = 0; i < slideCount; i++)
        {
            var rId = i + 1;
            rels.Append(
                $"""  <Relationship Id="rId{rId}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide{rId}.xml"/>{"\n"}""");
        }

        return string.Format(OoxmlTemplates.PresentationRelsTemplate, rels.ToString().TrimEnd('\n'));
    }

    private static string BuildSlideRels() => """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideLayout" Target="../slideLayouts/slideLayout1.xml"/>
        </Relationships>
        """;

    private static void WriteText(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(content);
    }
}
