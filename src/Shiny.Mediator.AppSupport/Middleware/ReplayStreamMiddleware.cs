using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Shiny.Mediator.Infrastructure;

namespace Shiny.Mediator.Middleware;


/// <summary>
/// Replays the last result before requesting a new one
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class ReplayStreamMiddleware<TRequest, TResult>(
    IStorageService storage,
    IConfiguration configuration
) : IStreamRequestMiddleware<TRequest, TResult> where TRequest : IStreamRequest<TResult>
{
    public IAsyncEnumerable<TResult> Process(
        TRequest request, 
        StreamRequestHandlerDelegate<TResult> next, 
        IStreamRequestHandler<TRequest, TResult> requestHandler,
        CancellationToken cancellationToken
    )
    {
        var section = configuration.GetHandlerSection("ReplayStream", request, this);
        var enabled = false;
        
        if (section == null)
        {
            enabled = requestHandler.GetHandlerHandleMethodAttribute<TRequest, ReplayStreamAttribute>() != null;
        }
        else
        {
            enabled = section.Get<bool>();
        }
        if (!enabled)
            return next();
        
        return this.Iterate(
            request, 
            requestHandler, 
            next, 
            cancellationToken
        );
    }


    protected virtual async IAsyncEnumerable<TResult> Iterate(
        TRequest request, 
        IStreamRequestHandler<TRequest, TResult> requestHandler, 
        StreamRequestHandlerDelegate<TResult> next, 
        [EnumeratorCancellation] CancellationToken ct
    )
    {
        var store = await storage.Get<TResult>(request);
        if (store != null)
            yield return store;

        var nxt = next().GetAsyncEnumerator(ct);
        while (await nxt.MoveNextAsync() && !ct.IsCancellationRequested)
        {
            // TODO: if current is null, remove?
            await storage.Store(request, nxt.Current!);
            yield return nxt.Current;
        }
    }
}