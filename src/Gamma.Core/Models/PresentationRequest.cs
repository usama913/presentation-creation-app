namespace Gamma.Core.Models;

/// <summary>The input to the AI drafting step: what Gamma calls "the prompt".</summary>
public sealed class PresentationRequest
{
    /// <summary>What the deck is about. Required.</summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>Desired number of slides, including title slide. Default 8.</summary>
    public int SlideCount { get; set; } = 8;

    /// <summary>Who the deck is for, e.g. "prospective enterprise customers". Optional.</summary>
    public string? Audience { get; set; }

    /// <summary>e.g. "formal", "playful", "technical". Optional.</summary>
    public string? Tone { get; set; }

    /// <summary>BCP-47-ish language name/tag, e.g. "English" or "es". Optional, defaults to English.</summary>
    public string? Language { get; set; }

    /// <summary>Optional theme override; if omitted the generator picks a sensible default.</summary>
    public ThemeOptions? Theme { get; set; }
}
