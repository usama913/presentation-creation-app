using Gamma.Core.Models;

namespace Gamma.Core.Abstractions;

/// <summary>
/// Turns a natural-language <see cref="PresentationRequest"/> into a
/// structured <see cref="PresentationDocument"/> using an LLM. This is
/// Gamma's "type a prompt, get a drafted deck" step.
/// </summary>
public interface ISlideContentGenerator
{
    Task<PresentationDocument> GenerateAsync(PresentationRequest request, CancellationToken cancellationToken = default);
}
