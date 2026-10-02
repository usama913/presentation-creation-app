using Gamma.Core.Models;

namespace Gamma.Core.Abstractions;

/// <summary>
/// Renders a format-agnostic <see cref="PresentationDocument"/> directly
/// to PDF (one page per slide) — Gamma's "export as PDF" path. This is a
/// direct PresentationDocument -&gt; PDF render, not a PPTX-&gt;PDF
/// conversion, so it never depends on a PowerPoint engine or a headless
/// Office/LibreOffice install being present on the host.
/// </summary>
public interface IPdfRenderer
{
    RendererProvider Provider { get; }

    Task<byte[]> RenderAsync(PresentationDocument document, CancellationToken cancellationToken = default);
}
