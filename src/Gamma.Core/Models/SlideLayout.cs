namespace Gamma.Core.Models;

/// <summary>
/// The visual arrangement of a single slide. Mirrors the small set of
/// layout primitives Gamma itself composes decks from.
/// </summary>
public enum SlideLayout
{
    /// <summary>Big centered title + subtitle. Typically the first slide.</summary>
    TitleSlide,

    /// <summary>A heading with a bulleted list of points underneath.</summary>
    TitleAndBullets,

    /// <summary>A large section-divider heading with no body content.</summary>
    SectionHeader,

    /// <summary>A short, emphasized quote or takeaway statement.</summary>
    Quote,

    /// <summary>A closing / thank-you / call-to-action slide.</summary>
    Closing
}
