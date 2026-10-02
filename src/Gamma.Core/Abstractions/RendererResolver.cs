using Gamma.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Gamma.Core.Abstractions;

/// <summary>
/// Resolves the right keyed <see cref="IPresentationRenderer"/> /
/// <see cref="IPdfRenderer"/> for a given <see cref="RendererProvider"/>.
/// This is the one place the API needs to know that "which renderer"
/// is a runtime choice, not a compile-time one — everything else just
/// depends on the interfaces.
/// </summary>
public sealed class RendererResolver
{
    private readonly IServiceProvider _serviceProvider;

    public RendererResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPresentationRenderer ResolvePresentationRenderer(RendererProvider provider) =>
        _serviceProvider.GetRequiredKeyedService<IPresentationRenderer>(provider);

    public IPdfRenderer ResolvePdfRenderer(RendererProvider provider) =>
        _serviceProvider.GetRequiredKeyedService<IPdfRenderer>(provider);
}
