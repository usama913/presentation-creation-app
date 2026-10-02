using Gamma.Ai.Clients;
using Gamma.Core.Abstractions;
using Gamma.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gamma.Ai;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the AI drafting stack. Reads Ai:Provider from configuration
    /// to decide whether OpenAI or Anthropic backs <see cref="ISlideContentGenerator"/>;
    /// both HttpClients are always registered (cheap) so switching provider
    /// is just a config change, no redeploy of different code.
    /// </summary>
    public static IServiceCollection AddGammaAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        services.AddHttpClient<OpenAiChatCompletionClient>();
        services.AddHttpClient<AnthropicChatCompletionClient>();

        services.AddScoped<IChatCompletionClient>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiOptions>>().Value;
            return options.Provider switch
            {
                AiProvider.Anthropic => sp.GetRequiredService<AnthropicChatCompletionClient>(),
                _ => sp.GetRequiredService<OpenAiChatCompletionClient>()
            };
        });

        services.AddScoped<ISlideContentGenerator, SlideContentGenerator>();

        return services;
    }
}
