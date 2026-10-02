using Gamma.Core.Abstractions;
using Gamma.Core.Models;

namespace Gamma.Rendering.Commercial;

/// <summary>
/// Placeholder for an Aspose-backed <see cref="IPdfRenderer"/>. Aspose.Slides
/// can export a Presentation directly to PDF (presentation.Save(stream,
/// Aspose.Slides.Export.SaveFormat.Pdf)), so in practice this can share the
/// same Presentation-building code as <see cref="AsposePresentationRenderer"/>
/// and just change the save format — see that class's doc comment.
/// </summary>
public sealed class AsposePdfRenderer : IPdfRenderer
{
    public RendererProvider Provider => RendererProvider.Aspose;

    public Task<byte[]> RenderAsync(PresentationDocument document, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Aspose.Slides is not wired up yet. Add the Aspose.Slides.NET NuGet package and implement " +
            "AsposePdfRenderer.RenderAsync, then set Rendering:DefaultProvider to \"Aspose\".");
    }
}
