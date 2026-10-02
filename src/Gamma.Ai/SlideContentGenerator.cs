using System.Text.Json;
using System.Text.Json.Serialization;
using Gamma.Ai.Clients;
using Gamma.Core.Abstractions;
using Gamma.Core.Models;
using Microsoft.Extensions.Logging;

namespace Gamma.Ai;

/// <summary>
/// Drives an <see cref="IChatCompletionClient"/> with a prompt that asks
/// for strict JSON matching <see cref="PresentationDocument"/>, then
/// parses the reply. This is the whole "AI drafts your deck" step.
/// </summary>
public sealed class SlideContentGenerator : ISlideContentGenerator
{
    private readonly IChatCompletionClient _chatClient;
    private readonly ILogger<SlideContentGenerator> _logger;

    public SlideContentGenerator(IChatCompletionClient chatClient, ILogger<SlideContentGenerator> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<PresentationDocument> GenerateAsync(PresentationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new ArgumentException("Prompt must not be empty.", nameof(request));
        }

        var slideCount = Math.Clamp(request.SlideCount <= 0 ? 8 : request.SlideCount, 1, 40);

        var systemPrompt = BuildSystemPrompt();
        var userPrompt = BuildUserPrompt(request, slideCount);

        var raw = await _chatClient.CompleteAsync(systemPrompt, userPrompt, cancellationToken);

        PresentationDocument document;
        try
        {
            document = ParseDocument(raw);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Model output was not valid JSON on first attempt; falling back to a minimal single-slide deck.");
            document = FallbackDocument(request);
        }

        if (request.Theme is not null)
        {
            document.Theme = request.Theme;
        }

        if (document.Slides.Count == 0)
        {
            document.Slides.Add(new Slide { Title = document.Title, Layout = SlideLayout.TitleSlide });
        }

        return document;
    }

    private static string BuildSystemPrompt() => """
        You are a presentation writer. Given a topic, you produce the CONTENT
        of a slide deck as a single strict JSON object — no prose, no markdown
        fences, no commentary before or after the JSON.

        The JSON object MUST have exactly this shape:
        {
          "title": string,
          "subtitle": string | null,
          "slides": [
            {
              "title": string,
              "bullets": string[],       // 0-6 short bullet points, plain text, no leading dashes
              "body": string | null,     // used instead of bullets for quote/closing/section slides
              "layout": "TitleSlide" | "TitleAndBullets" | "SectionHeader" | "Quote" | "Closing",
              "speakerNotes": string | null
            }
          ]
        }

        Rules:
        - The first slide must use layout "TitleSlide" and have empty bullets.
        - Bullets must be concise (under ~12 words each), no sub-bullets.
        - Use "SectionHeader" slides sparingly to break the deck into parts if it's long.
        - The last slide should use layout "Closing" with a short call-to-action in "body".
        - Return ONLY the JSON object, nothing else.
        """;

    private static string BuildUserPrompt(PresentationRequest request, int slideCount)
    {
        var audience = string.IsNullOrWhiteSpace(request.Audience) ? "a general audience" : request.Audience;
        var tone = string.IsNullOrWhiteSpace(request.Tone) ? "clear and professional" : request.Tone;
        var language = string.IsNullOrWhiteSpace(request.Language) ? "English" : request.Language;

        return $"""
            Topic: {request.Prompt}
            Audience: {audience}
            Tone: {tone}
            Language: {language}
            Number of slides (including the title slide): {slideCount}
            """;
    }

    private static PresentationDocument ParseDocument(string raw)
    {
        var json = ExtractJsonObject(raw);
        var dto = JsonSerializer.Deserialize<GeneratedDocumentDto>(json, JsonOptions)
                  ?? throw new JsonException("Deserialization returned null.");

        return new PresentationDocument
        {
            Title = dto.Title ?? "Untitled presentation",
            Subtitle = dto.Subtitle,
            Slides = (dto.Slides ?? new()).Select(s => new Slide
            {
                Title = s.Title ?? string.Empty,
                Bullets = s.Bullets ?? new List<string>(),
                Body = s.Body,
                SpeakerNotes = s.SpeakerNotes,
                Layout = ParseLayout(s.Layout)
            }).ToList()
        };
    }

    /// <summary>
    /// Some models wrap JSON in ```json fences despite instructions; strip
    /// anything before the first '{' and after the matching last '}'.
    /// </summary>
    private static string ExtractJsonObject(string raw)
    {
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        if (start < 0 || end < 0 || end < start)
        {
            throw new JsonException("No JSON object found in model output.");
        }

        return raw.Substring(start, end - start + 1);
    }

    private static SlideLayout ParseLayout(string? value) =>
        Enum.TryParse<SlideLayout>(value, ignoreCase: true, out var layout) ? layout : SlideLayout.TitleAndBullets;

    private static PresentationDocument FallbackDocument(PresentationRequest request) => new()
    {
        Title = request.Prompt,
        Slides = new List<Slide>
        {
            new() { Title = request.Prompt, Layout = SlideLayout.TitleSlide },
            new()
            {
                Title = "Overview",
                Layout = SlideLayout.TitleAndBullets,
                Bullets = new List<string> { "Content generation did not return valid structured output.", "Please retry, or edit this deck manually." }
            }
        }
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class GeneratedDocumentDto
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("subtitle")] public string? Subtitle { get; set; }
        [JsonPropertyName("slides")] public List<GeneratedSlideDto>? Slides { get; set; }
    }

    private sealed class GeneratedSlideDto
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("bullets")] public List<string>? Bullets { get; set; }
        [JsonPropertyName("body")] public string? Body { get; set; }
        [JsonPropertyName("layout")] public string? Layout { get; set; }
        [JsonPropertyName("speakerNotes")] public string? SpeakerNotes { get; set; }
    }
}
