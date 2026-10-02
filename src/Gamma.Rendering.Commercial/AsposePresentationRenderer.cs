using Gamma.Core.Abstractions;
using Gamma.Core.Models;

namespace Gamma.Rendering.Commercial;

/// <summary>
/// Placeholder for an Aspose.Slides-backed <see cref="IPresentationRenderer"/>.
/// Aspose.Slides is a commercial engine (paid license) that gives you far
/// richer layout/design fidelity than the hand-rolled OpenSource renderer
/// (real placeholder inheritance, charts, SmartArt-like shapes, direct
/// PPTX-&gt;PDF/PPTX-&gt;image conversion, etc.).
///
/// To activate:
///   1. dotnet add package Aspose.Slides.NET   (or Aspose.Slides for a specific TFM)
///   2. using Aspose.Slides;
///   3. Build a Presentation, add one slide per <see cref="Slide"/>, map
///      SlideLayout -> a real layout/shape composition, then
///      presentation.Save(stream, Aspose.Slides.Export.SaveFormat.Pptx).
///   4. Register this class for RendererProvider.Aspose in
///      ServiceCollectionExtensions.AddCommercialRendering below and set
///      Rendering:DefaultProvider (or the per-request ?renderer= override)
///      to "Aspose".
/// </summary>
public sealed class AsposePresentationRenderer : IPresentationRenderer
{
    public RendererProvider Provider => RendererProvider.Aspose;

    public Task<byte[]> RenderAsync(PresentationDocument document, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Aspose.Slides is not wired up yet. Add the Aspose.Slides.NET NuGet package and implement " +
            "AsposePresentationRenderer.RenderAsync (see the class doc comment for the shape of the code), " +
            "then set Rendering:DefaultProvider to \"Aspose\".");
    }
}
