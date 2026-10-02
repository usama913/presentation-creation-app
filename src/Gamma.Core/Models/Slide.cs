namespace Gamma.Core.Models;

/// <summary>A single slide's content, independent of any file format.</summary>
public sealed class Slide
{
    public string Title { get; set; } = string.Empty;

    /// <summary>Bullet points for <see cref="SlideLayout.TitleAndBullets"/> slides.</summary>
    public List<string> Bullets { get; set; } = new();

    /// <summary>Longer body text, used by <see cref="SlideLayout.Quote"/> / <see cref="SlideLayout.Closing"/>.</summary>
    public string? Body { get; set; }

    public SlideLayout Layout { get; set; } = SlideLayout.TitleAndBullets;

    /// <summary>
    /// Speaker notes. Not rendered into the deck body; renderers may write
    /// these into the PPTX notes part when supported.
    /// </summary>
    public string? SpeakerNotes { get; set; }

    /// <summary>
    /// A free-text description of an illustrative image for this slide.
    /// Reserved for a future image-generation step; current renderers
    /// ignore it safely.
    /// </summary>
    public string? ImagePrompt { get; set; }
}
