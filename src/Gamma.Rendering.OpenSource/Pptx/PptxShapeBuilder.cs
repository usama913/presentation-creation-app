using System.Globalization;
using System.Text;

namespace Gamma.Rendering.OpenSource.Pptx;

/// <summary>
/// Builds the DrawingML fragments for a single text-box shape. Slides in
/// this renderer never use layout-inherited placeholders — every shape
/// carries its own explicit position/size (a:xfrm) and formatting, which
/// keeps the layout/master parts trivially simple and every shape
/// self-describing.
/// </summary>
internal static class PptxShapeBuilder
{
    public readonly record struct Run(string Text, bool Italic = false);

    public readonly record struct Paragraph(IReadOnlyList<Run> Runs, bool Bulleted, string Align = "l");

    /// <param name="id">Shape id, unique within the slide (2, 3, 4, ... — 1 is reserved for the group shape).</param>
    /// <param name="x">Left offset in EMU.</param>
    /// <param name="y">Top offset in EMU.</param>
    /// <param name="cx">Width in EMU.</param>
    /// <param name="cy">Height in EMU.</param>
    /// <param name="fontSizeHundredths">Font size in hundredths of a point (e.g. 3200 = 32pt).</param>
    /// <param name="colorHex">RGB hex, no '#'.</param>
    /// <param name="bold">Whether every run in this shape is bold.</param>
    /// <param name="fontFamily">Latin typeface name.</param>
    /// <param name="verticalAnchor">"t" top, "ctr" center, "b" bottom.</param>
    public static string TextBox(
        int id,
        long x, long y, long cx, long cy,
        IReadOnlyList<Paragraph> paragraphs,
        int fontSizeHundredths,
        string colorHex,
        bool bold,
        string fontFamily,
        string verticalAnchor = "t")
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"""
            <p:sp>
              <p:nvSpPr>
                <p:cNvPr id="{id}" name="TextBox {id}"/>
                <p:cNvSpPr txBox="1"/>
                <p:nvPr/>
              </p:nvSpPr>
              <p:spPr>
                <a:xfrm><a:off x="{x}" y="{y}"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm>
                <a:prstGeom prst="rect"><a:avLst/></a:prstGeom>
                <a:noFill/>
              </p:spPr>
              <p:txBody>
                <a:bodyPr wrap="square" anchor="{verticalAnchor}"><a:normAutofit/></a:bodyPr>
                <a:lstStyle/>

            """);

        foreach (var paragraph in paragraphs)
        {
            AppendParagraph(sb, paragraph, fontSizeHundredths, colorHex, bold, fontFamily);
        }

        sb.Append("""
              </p:txBody>
            </p:sp>
            """);

        return sb.ToString();
    }

    private static void AppendParagraph(StringBuilder sb, Paragraph paragraph, int fontSizeHundredths, string colorHex, bool bold, string fontFamily)
    {
        sb.Append("    <a:p>\n");

        if (paragraph.Bulleted)
        {
            sb.Append(CultureInfo.InvariantCulture, $"""
                      <a:pPr marL="342900" indent="-342900" algn="{paragraph.Align}">
                        <a:buFont typeface="Arial"/>
                        <a:buChar char="&#8226;"/>
                      </a:pPr>

                """);
        }
        else
        {
            sb.Append(CultureInfo.InvariantCulture, $"""
                      <a:pPr algn="{paragraph.Align}"><a:buNone/></a:pPr>

                """);
        }

        if (paragraph.Runs.Count == 0)
        {
            // An empty paragraph still needs an end-paragraph run properties element to render a blank line consistently.
            sb.Append(CultureInfo.InvariantCulture, $"""      <a:endParaRPr lang="en-US" sz="{fontSizeHundredths}"/>{"\n"}""");
        }
        else
        {
            foreach (var run in paragraph.Runs)
            {
                var italicAttr = run.Italic ? " i=\"1\"" : "";
                sb.Append(CultureInfo.InvariantCulture, $"""
                          <a:r>
                            <a:rPr lang="en-US" sz="{fontSizeHundredths}" b="{(bold ? 1 : 0)}"{italicAttr} dirty="0">
                              <a:solidFill><a:srgbClr val="{colorHex}"/></a:solidFill>
                              <a:latin typeface="{XmlText.Escape(fontFamily)}"/>
                            </a:rPr>
                            <a:t>{XmlText.Escape(run.Text)}</a:t>
                          </a:r>

                    """);
            }
        }

        sb.Append("    </a:p>\n");
    }
}
