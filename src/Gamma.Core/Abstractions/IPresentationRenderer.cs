using Gamma.Core.Models;

namespace Gamma.Core.Abstractions;

/// <summary>
/// Renders a format-agnostic <see cref="PresentationDocument"/> into a
/// PowerPoint (.pptx) file. Every implementation — open-source, Aspose,
/// Syncfusion — is a drop-in replacement behind this one contract, so the
/// API and the AI drafting layer never need to know which engine is
/// actually producing the bytes.
/// </summary>
public interface IPresentationRenderer
{
    /// <summary>Which backend this implementation is ("OpenSource", "Aspose", "Syncfusion", ...). Matches <see cref="RendererProvider"/>.</summary>
    RendererProvider Provider { get; }

    Task<byte[]> RenderAsync(PresentationDocument document, CancellationToken cancellationToken = default);
}
