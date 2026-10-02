namespace Gamma.Ai.Clients;

/// <summary>
/// The one thing every LLM provider needs to offer for our purposes: given
/// a system instruction and a user message, return the model's raw text
/// reply. Deliberately narrow — this is not a general chat SDK, it's just
/// enough surface to drive the slide-drafting prompt.
/// </summary>
public interface IChatCompletionClient
{
    /// <summary>Provider name, e.g. "OpenAI" or "Anthropic". Useful for logging.</summary>
    string ProviderName { get; }

    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
