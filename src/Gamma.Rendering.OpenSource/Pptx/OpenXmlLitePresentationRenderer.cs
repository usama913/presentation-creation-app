using Gamma.Core.Abstractions;
using Gamma.Core.Models;

namespace Gamma.Rendering.OpenSource.Pptx;

/// <summary>
/// The default, free, dependency-free <see cref="IPresentationRenderer"/>.
/// Produces a real .pptx file by hand-assembling the OOXML package — see
/// <see cref="MinimalPptxWriter"/>.
/// </summary>
public sealed class OpenXmlLitePresentationRenderer : IPresentationRenderer
{
    public RendererProvider Provider => RendererProvider.OpenSource;

    public Task<byte[]> RenderAsync(PresentationDocument document, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bytes = MinimalPptxWriter.Write(document);
        return Task.FromResult(bytes);
    }
}
