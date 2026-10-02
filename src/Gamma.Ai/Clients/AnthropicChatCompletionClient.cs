using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gamma.Core.Models;
using Microsoft.Extensions.Options;

namespace Gamma.Ai.Clients;

/// <summary>Talks to Anthropic's Messages API directly over HTTP.</summary>
public sealed class AnthropicChatCompletionClient : IChatCompletionClient
{
    private readonly HttpClient _http;
    private readonly AnthropicOptions _options;

    public string ProviderName => "Anthropic";

    public AnthropicChatCompletionClient(HttpClient http, IOptions<AiOptions> aiOptions)
    {
        _http = http;
        _options = aiOptions.Value.Anthropic;

        if (_http.BaseAddress is null)
        {
            var baseUrl = _options.BaseUrl.EndsWith('/') ? _options.BaseUrl : _options.BaseUrl + "/";
            _http.BaseAddress = new Uri(baseUrl);
        }

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _http.DefaultRequestHeaders.Remove("x-api-key");
            _http.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        }

        _http.DefaultRequestHeaders.Remove("anthropic-version");
        _http.DefaultRequestHeaders.Add("anthropic-version", _options.ApiVersion);
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "Anthropic API key is not configured. Set Ai:Anthropic:ApiKey in appsettings/user-secrets, " +
                "or the ANTHROPIC_API_KEY environment variable mapped to it.");
        }

        var payload = new AnthropicRequest
        {
            Model = _options.Model,
            System = systemPrompt,
            MaxTokens = 4096,
            Messages = new List<AnthropicMessage>
            {
                new() { Role = "user", Content = userPrompt }
            }
        };

        using var response = await _http.PostAsJsonAsync("messages", payload, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Anthropic request failed ({(int)response.StatusCode} {response.StatusCode}): {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<AnthropicResponse>(JsonOptions, cancellationToken);
        var text = result?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Anthropic response contained no text content.");
        }

        return text;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed class AnthropicRequest
    {
        [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
        [JsonPropertyName("system")] public string? System { get; set; }
        [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
        [JsonPropertyName("messages")] public List<AnthropicMessage> Messages { get; set; } = new();
    }

    private sealed class AnthropicMessage
    {
        [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
        [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
    }

    private sealed class AnthropicResponse
    {
        [JsonPropertyName("content")] public List<AnthropicContentBlock>? Content { get; set; }
    }

    private sealed class AnthropicContentBlock
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("text")] public string? Text { get; set; }
    }
}
