namespace Gamma.Core.Models;

/// <summary>Bound from the "Ai" section of appsettings.json / environment / user-secrets.</summary>
public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public AiProvider Provider { get; set; } = AiProvider.OpenAi;

    public OpenAiOptions OpenAi { get; set; } = new();

    public AnthropicOptions Anthropic { get; set; } = new();
}

public sealed class OpenAiOptions
{
    /// <summary>Read from config; prefer the OPENAI_API_KEY environment variable in production.</summary>
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Override to point at an Azure OpenAI-compatible endpoint or a local
    /// gateway. Must be OpenAI Chat Completions-compatible.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
}

public sealed class AnthropicOptions
{
    /// <summary>Read from config; prefer the ANTHROPIC_API_KEY environment variable in production.</summary>
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-sonnet-4-5";

    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1/";

    public string ApiVersion { get; set; } = "2023-06-01";
}
