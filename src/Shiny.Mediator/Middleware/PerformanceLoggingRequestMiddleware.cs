using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shiny.Mediator.Infrastructure;

namespace Shiny.Mediator.Middleware;


public class PerformanceLoggingRequestMiddleware<TRequest, TResult>(
    IConfiguration configuration,
    ILogger<TRequest> logger
) : IRequestMiddleware<TRequest, TResult>
{
    public async Task<TResult> Process(
        RequestContext<TRequest> context, 
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken
    )
    {
        var section = configuration.GetHandlerSection("PerformanceLogging", context.Request!, context.Handler);
        if (section == null)
            return await next().ConfigureAwait(false);

        var millis = section.GetValue("ErrorThresholdMilliseconds", 5000);
        var ts = TimeSpan.FromMilliseconds(millis);
        var startTime = Stopwatch.GetTimestamp();
        var result = await next();
        var delta = Stopwatch.GetElapsedTime(startTime);

        if (delta > ts)
        {
            context.SetPerformanceLoggingThresholdBreached(delta);
            logger.LogError(
                "{RequestType} took longer than {Threshold} to execute - {Elapsed}", 
                typeof(TRequest), 
                ts,
                delta
            );
        }
        else if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "{RequestType} took {Elapsed} to execute", 
                typeof(TRequest), 
                delta
            );
        }

        return result;
    }
}

