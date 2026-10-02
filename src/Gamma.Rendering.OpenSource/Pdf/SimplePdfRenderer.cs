using Gamma.Core.Abstractions;
using Gamma.Core.Models;

namespace Gamma.Rendering.OpenSource.Pdf;

/// <summary>
/// The default, free, dependency-free <see cref="IPdfRenderer"/>. Renders
/// directly from <see cref="PresentationDocument"/> to PDF (one page per
/// slide) — see <see cref="MinimalPdfWriter"/>. Never converts a .pptx;
/// there is no LibreOffice/Office dependency anywhere in this path.
/// </summary>
public sealed class SimplePdfRenderer : IPdfRenderer
{
    public RendererProvider Provider => RendererProvider.OpenSource;

    public Task<byte[]> RenderAsync(PresentationDocument document, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bytes = MinimalPdfWriter.Write(document);
        return Task.FromResult(bytes);
    }
}
