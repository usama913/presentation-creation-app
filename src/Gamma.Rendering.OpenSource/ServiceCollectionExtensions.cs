using Gamma.Core.Abstractions;
using Gamma.Core.Models;
using Gamma.Rendering.OpenSource.Pdf;
using Gamma.Rendering.OpenSource.Pptx;
using Microsoft.Extensions.DependencyInjection;

namespace Gamma.Rendering.OpenSource;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the free, dependency-free renderers under the
    /// <see cref="RendererProvider.OpenSource"/> key. Keyed registration
    /// (not just a single default binding) is what lets the API resolve a
    /// specific engine per request/config without an if/else chain.
    /// </summary>
    public static IServiceCollection AddOpenSourceRendering(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IPresentationRenderer, OpenXmlLitePresentationRenderer>(RendererProvider.OpenSource);
        services.AddKeyedSingleton<IPdfRenderer, SimplePdfRenderer>(RendererProvider.OpenSource);
        return services;
    }
}
