namespace Gamma.Core.Models;

/// <summary>
/// The format-agnostic content of a deck: what both the PPTX renderer and
/// the PDF renderer render from. This is the object the AI drafting step
/// produces, and the object a caller can also hand-build/edit directly
/// (e.g. from their own JSON) without touching any AI provider at all.
/// </summary>
public sealed class PresentationDocument
{
    public string Title { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    public List<Slide> Slides { get; set; } = new();

    public ThemeOptions Theme { get; set; } = ThemeOptions.Default;
}
