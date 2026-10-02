namespace Gamma.Core.Models;

/// <summary>Bound from the "Rendering" section of appsettings.json.</summary>
public sealed class RenderingOptions
{
    public const string SectionName = "Rendering";

    /// <summary>The default engine used when a request doesn't specify one explicitly.</summary>
    public RendererProvider DefaultProvider { get; set; } = RendererProvider.OpenSource;
}
