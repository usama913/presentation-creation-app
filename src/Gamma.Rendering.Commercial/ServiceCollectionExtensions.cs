using Gamma.Core.Abstractions;
using Gamma.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Gamma.Rendering.Commercial;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the commercial-engine stubs under their
    /// <see cref="RendererProvider"/> keys. Safe to always call — these
    /// throw <see cref="NotImplementedException"/> only when actually
    /// invoked (i.e. only if something asks for RendererProvider.Aspose or
    /// .Syncfusion before you've implemented them), never at startup.
    /// </summary>
    public static IServiceCollection AddCommercialRendering(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IPresentationRenderer, AsposePresentationRenderer>(RendererProvider.Aspose);
        services.AddKeyedSingleton<IPdfRenderer, AsposePdfRenderer>(RendererProvider.Aspose);

        services.AddKeyedSingleton<IPresentationRenderer, SyncfusionPresentationRenderer>(RendererProvider.Syncfusion);
        services.AddKeyedSingleton<IPdfRenderer, SyncfusionPdfRenderer>(RendererProvider.Syncfusion);

        return services;
    }
}
