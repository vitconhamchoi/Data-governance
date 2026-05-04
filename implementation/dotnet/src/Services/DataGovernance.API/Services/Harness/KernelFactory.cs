using DataGovernance.API.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace DataGovernance.API.Services.Harness;

public interface IKernelFactory
{
    Kernel CreateKernel(string provider, string model);
}

public sealed class KernelFactory : IKernelFactory
{
    private readonly HarnessOptions _options;

    public KernelFactory(IOptions<HarnessOptions> options)
    {
        _options = options.Value;
    }

    public Kernel CreateKernel(string provider, string model)
    {
        var builder = Kernel.CreateBuilder();

        var normalizedProvider = provider.Trim().ToLowerInvariant();
        if (normalizedProvider == "anthropic")
        {
            if (string.IsNullOrWhiteSpace(_options.Anthropic.ApiKey))
            {
                throw new InvalidOperationException("Anthropic ApiKey is not configured.");
            }

            // Anthropic connector availability depends on package/runtime.
            // Fallback to OpenAI connector style endpoint only when explicit base model is set.
            builder.AddOpenAIChatCompletion(
                modelId: string.IsNullOrWhiteSpace(model) ? _options.Anthropic.Model : model,
                apiKey: _options.Anthropic.ApiKey);

            return builder.Build();
        }

        if (string.IsNullOrWhiteSpace(_options.OpenAI.ApiKey))
        {
            throw new InvalidOperationException("OpenAI ApiKey is not configured.");
        }

        builder.AddOpenAIChatCompletion(
            modelId: string.IsNullOrWhiteSpace(model) ? _options.OpenAI.Model : model,
            apiKey: _options.OpenAI.ApiKey);

        return builder.Build();
    }
}
