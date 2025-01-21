using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Shiny.Mediator.Resilience.Handlers;


public class ResilientCommandMiddleware<TCommand>(
    ILogger<ResilientCommandMiddleware<TCommand>> logger,
    IConfiguration configuration,
    ResiliencePipelineProvider<string> pipelineProvider
) : ICommandMiddleware<TCommand> where TCommand : ICommand
{
    public async Task Process(
        CommandContext<TCommand> context,
        CommandHandlerDelegate next,
        CancellationToken cancellationToken
    )
    {
        ResiliencePipeline? pipeline = null;
        var section = configuration.GetHandlerSection("Resilience", context.Command, context.Handler);
        
        if (section != null)
        {
            pipeline = pipelineProvider.GetPipeline(section.Key.ToLower());
        }
        else
        {
            var attribute = context.Handler.GetHandlerHandleMethodAttribute<TCommand, ResilientAttribute>();
            if (attribute != null)
                pipeline = pipelineProvider.GetPipeline(attribute.ConfigurationKey.ToLower());
        }

        if (pipeline == null)
        {
            await next().ConfigureAwait(false);
            return;
        }

        // it can't cancel properly here... may need to make next take a CancellationToken
        logger.LogDebug("Resilience Enabled - {Request}", context.Command);
        await pipeline
            .ExecuteAsync(async _ => await next(), cancellationToken)
            .ConfigureAwait(false);
    }
}