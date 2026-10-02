namespace Gamma.Core.Models;

/// <summary>
/// Which rendering backend produces the actual .pptx / .pdf bytes.
/// <see cref="OpenSource"/> is fully self-contained (no paid license, no
/// external package). <see cref="Aspose"/> and <see cref="Syncfusion"/> are
/// commercial engines you can switch on later just by installing their
/// NuGet package and implementing the corresponding stub in
/// Gamma.Rendering.Commercial — no changes to the API or domain model are
/// required.
/// </summary>
public enum RendererProvider
{
    OpenSource,
    Aspose,
    Syncfusion
}
