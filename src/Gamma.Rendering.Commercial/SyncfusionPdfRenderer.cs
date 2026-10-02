using Gamma.Core.Abstractions;
using Gamma.Core.Models;

namespace Gamma.Rendering.Commercial;

/// <summary>
/// Placeholder for a Syncfusion-backed <see cref="IPdfRenderer"/>.
/// Syncfusion.Presentation can convert a presentation to PDF via its
/// PresentationToPdfConverter — see <see cref="SyncfusionPresentationRenderer"/>
/// for the general shape of building the document first.
/// </summary>
public sealed class SyncfusionPdfRenderer : IPdfRenderer
{
    public RendererProvider Provider => RendererProvider.Syncfusion;

    public Task<byte[]> RenderAsync(PresentationDocument document, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Syncfusion.Presentation is not wired up yet. Add the Syncfusion.Presentation.Net.Core NuGet " +
            "package and implement SyncfusionPdfRenderer.RenderAsync, then set Rendering:DefaultProvider " +
            "to \"Syncfusion\".");
    }
}
