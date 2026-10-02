using Gamma.Core.Models;

namespace Gamma.Api.Contracts;

/// <summary>Body for the one-shot "prompt in, file out" endpoint.</summary>
public sealed class CreatePresentationRequest
{
    public string Prompt { get; set; } = string.Empty;

    public int SlideCount { get; set; } = 8;

    public string? Audience { get; set; }

    public string? Tone { get; set; }

    public string? Language { get; set; }

    public ThemeOptions? Theme { get; set; }

    public PresentationRequest ToDomain() => new()
    {
        Prompt = Prompt,
        SlideCount = SlideCount,
        Audience = Audience,
        Tone = Tone,
        Language = Language,
        Theme = Theme
    };
}
