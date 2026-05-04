using DataGovernance.API.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Azure.AI.OpenAI;
using Azure;

namespace DataGovernance.API.Services.Harness;

public interface IChatClientFactory
{
    IChatClient CreateChatClient(string provider, string model);
}

public sealed class ChatClientFactory : IChatClientFactory
{
    private readonly HarnessOptions _options;

    public ChatClientFactory(IOptions<HarnessOptions> options)
    {
        _options = options.Value;
    }

    public IChatClient CreateChatClient(string provider, string model)
    {
        var normalizedProvider = provider.Trim().ToLowerInvariant();

        if (normalizedProvider == "anthropic")
        {
            if (string.IsNullOrWhiteSpace(_options.Anthropic.ApiKey))
            {
                throw new InvalidOperationException("Anthropic ApiKey is not configured.");
            }

            if (!_options.Anthropic.UseOpenAICompatibleProxy)
            {
                throw new NotSupportedException(
                    "Anthropic direct connector is not configured. Set Harness:Anthropic:UseOpenAICompatibleProxy=true when using an OpenAI-compatible Anthropic gateway.");
            }

            var modelId = string.IsNullOrWhiteSpace(model) ? _options.Anthropic.Model : model;

            // Use OpenAI-compatible endpoint for Anthropic
            var client = new AzureOpenAIClient(
                new Uri("https://api.anthropic.com/v1"),
                new AzureKeyCredential(_options.Anthropic.ApiKey));

            return client.AsChatClient(modelId);
        }

        if (string.IsNullOrWhiteSpace(_options.OpenAI.ApiKey))
        {
            throw new InvalidOperationException("OpenAI ApiKey is not configured.");
        }

        var openAIModelId = string.IsNullOrWhiteSpace(model) ? _options.OpenAI.Model : model;

        // Use OpenAI chat client through Microsoft.Extensions.AI
        var openAIClient = new AzureOpenAIClient(
            new Uri("https://api.openai.com/v1"),
            new AzureKeyCredential(_options.OpenAI.ApiKey));

        return openAIClient.AsChatClient(openAIModelId);
    }
}
