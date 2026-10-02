using System.Globalization;
using System.Text;

namespace Gamma.Rendering.OpenSource.Pdf;

/// <summary>
/// Low-level byte-exact PDF writer: object numbering, xref table, trailer.
/// Every character appended is restricted to Latin-1 (0-255) by the
/// callers (see <see cref="PdfTextSanitizer"/>), so tracking byte offsets
/// as string length works — one .NET char in this builder is always one
/// output byte.
/// </summary>
internal sealed class PdfDocumentWriter
{
    private readonly StringBuilder _buffer = new();
    private readonly List<int> _offsets = new(); // index 0 unused; offsets[n] = byte offset of object n

    public PdfDocumentWriter()
    {
        _buffer.Append("%PDF-1.4\n");
    }

    /// <summary>Reserves object numbers 1..count and returns the total object count so far including these.</summary>
    public int NextObjectNumber => _offsets.Count + 1;

    /// <summary>Writes a complete "N 0 obj &lt;&lt;...&gt;&gt; endobj" for a dictionary object.</summary>
    public int WriteDictionaryObject(string dictionaryBody)
    {
        var number = BeginObject();
        _buffer.Append("<<").Append(dictionaryBody).Append(">>\nendobj\n");
        return number;
    }

    /// <summary>Writes a complete stream object: "N 0 obj &lt;&lt;dict&gt;&gt; stream\n...\nendstream endobj".</summary>
    public int WriteStreamObject(string extraDictEntries, string streamContent)
    {
        var number = BeginObject();
        var length = Encoding.Latin1.GetByteCount(streamContent);
        _buffer.Append("<<").Append(extraDictEntries).Append(" /Length ").Append(length).Append(">>\n");
        _buffer.Append("stream\n");
        _buffer.Append(streamContent);
        if (streamContent.Length == 0 || streamContent[^1] != '\n')
        {
            _buffer.Append('\n');
        }
        _buffer.Append("endstream\nendobj\n");
        return number;
    }

    private int BeginObject()
    {
        var number = NextObjectNumber;
        _offsets.Add(_buffer.Length);
        _buffer.Append(number).Append(" 0 obj\n");
        return number;
    }

    /// <summary>Finalizes the file: xref table, trailer, startxref, %%EOF. Returns the full byte content.</summary>
    public byte[] Finish(int catalogObjectNumber)
    {
        var xrefOffset = _buffer.Length;
        var totalObjects = _offsets.Count;

        _buffer.Append("xref\n");
        _buffer.Append(CultureInfo.InvariantCulture, $"0 {totalObjects + 1}\n");
        _buffer.Append("0000000000 65535 f\r\n");

        for (var i = 0; i < totalObjects; i++)
        {
            _buffer.Append(_offsets[i].ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n\r\n");
        }

        _buffer.Append("trailer\n");
        _buffer.Append("<<");
        _buffer.Append(CultureInfo.InvariantCulture, $"/Size {totalObjects + 1} /Root {catalogObjectNumber} 0 R");
        _buffer.Append(">>\n");
        _buffer.Append("startxref\n");
        _buffer.Append(xrefOffset).Append('\n');
        _buffer.Append("%%EOF");

        return Encoding.Latin1.GetBytes(_buffer.ToString());
    }
}
