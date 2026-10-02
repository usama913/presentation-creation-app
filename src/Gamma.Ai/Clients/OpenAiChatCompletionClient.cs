using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gamma.Core.Models;
using Microsoft.Extensions.Options;

namespace Gamma.Ai.Clients;

/// <summary>
/// Talks to OpenAI's Chat Completions endpoint directly over HTTP. Also
/// works against any OpenAI-compatible endpoint (Azure OpenAI's
/// chat-completions-compatible route, a self-hosted gateway, etc.) by
/// overriding <see cref="OpenAiOptions.BaseUrl"/>.
/// </summary>
public sealed class OpenAiChatCompletionClient : IChatCompletionClient
{
    private readonly HttpClient _http;
    private readonly OpenAiOptions _options;

    public string ProviderName => "OpenAI";

    public OpenAiChatCompletionClient(HttpClient http, IOptions<AiOptions> aiOptions)
    {
        _http = http;
        _options = aiOptions.Value.OpenAi;

        if (_http.BaseAddress is null)
        {
            var baseUrl = _options.BaseUrl.EndsWith('/') ? _options.BaseUrl : _options.BaseUrl + "/";
            _http.BaseAddress = new Uri(baseUrl);
        }

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured. Set Ai:OpenAi:ApiKey in appsettings/user-secrets, " +
                "or the OPENAI_API_KEY environment variable mapped to it.");
        }

        var payload = new OpenAiChatRequest
        {
            Model = _options.Model,
            Messages = new List<OpenAiMessage>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = userPrompt }
            },
            Temperature = 0.7,
            // Ask for a plain JSON object back; the caller is responsible for
            // prompting the model to emit an object matching its schema.
            ResponseFormat = new OpenAiResponseFormat { Type = "json_object" }
        };

        using var response = await _http.PostAsJsonAsync("chat/completions", payload, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"OpenAI request failed ({(int)response.StatusCode} {response.StatusCode}): {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>(JsonOptions, cancellationToken);
        var text = result?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("OpenAI response contained no content.");
        }

        return text;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed class OpenAiChatRequest
    {
        [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
        [JsonPropertyName("messages")] public List<OpenAiMessage> Messages { get; set; } = new();
        [JsonPropertyName("temperature")] public double Temperature { get; set; }
        [JsonPropertyName("response_format")] public OpenAiResponseFormat? ResponseFormat { get; set; }
    }

    private sealed class OpenAiResponseFormat
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "json_object";
    }

    private sealed class OpenAiMessage
    {
        [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
        [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
    }

    private sealed class OpenAiChatResponse
    {
        [JsonPropertyName("choices")] public List<OpenAiChoice>? Choices { get; set; }
    }

    private sealed class OpenAiChoice
    {
        [JsonPropertyName("message")] public OpenAiMessage? Message { get; set; }
    }
}
