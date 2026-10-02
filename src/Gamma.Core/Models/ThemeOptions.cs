namespace Gamma.Core.Models;

/// <summary>
/// A minimal brand theme. Deliberately small today (three knobs) but this
/// is the seam where a real theme system (named presets, logos, font
/// pairs, background images) grows without touching the renderers' public
/// contracts.
/// </summary>
public sealed class ThemeOptions
{
    /// <summary>Hex color, e.g. "1A73E8", no leading '#'.</summary>
    public string PrimaryColorHex { get; set; } = "1F2933";

    /// <summary>Hex color used for accents/headings, no leading '#'.</summary>
    public string AccentColorHex { get; set; } = "3B82F6";

    /// <summary>Hex color used for slide backgrounds, no leading '#'.</summary>
    public string BackgroundColorHex { get; set; } = "FFFFFF";

    /// <summary>Display name only today; real font embedding is a future step.</summary>
    public string FontFamily { get; set; } = "Calibri";

    public static ThemeOptions Default => new();
}
