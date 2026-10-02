using Gamma.Core.Abstractions;
using Gamma.Core.Models;

namespace Gamma.Rendering.Commercial;

/// <summary>
/// Placeholder for a Syncfusion.Presentation-backed <see cref="IPresentationRenderer"/>.
/// Syncfusion's Essential DocIO/Presentation suite is a commercial engine
/// (paid license, though it has a free Community License for small
/// companies/individuals under Syncfusion's eligibility rules).
///
/// To activate:
///   1. dotnet add package Syncfusion.Presentation.Net.Core
///   2. using Syncfusion.Presentation;
///   3. IPresentation pptxDoc = Presentation.Create(); build one ISlide per
///      <see cref="Slide"/>, mapping SlideLayout -> shapes/placeholders,
///      then pptxDoc.Save(stream).
///   4. Register this class for RendererProvider.Syncfusion in
///      ServiceCollectionExtensions.AddCommercialRendering below and set
///      Rendering:DefaultProvider (or the per-request ?renderer= override)
///      to "Syncfusion".
/// </summary>
public sealed class SyncfusionPresentationRenderer : IPresentationRenderer
{
    public RendererProvider Provider => RendererProvider.Syncfusion;

    public Task<byte[]> RenderAsync(PresentationDocument document, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Syncfusion.Presentation is not wired up yet. Add the Syncfusion.Presentation.Net.Core NuGet " +
            "package and implement SyncfusionPresentationRenderer.RenderAsync, then set " +
            "Rendering:DefaultProvider to \"Syncfusion\".");
    }
}
