namespace Shiny.Mediator.Infrastructure;

public class SentryEventMiddleware<TEvent>(IHub hub) : IEventMiddleware<TEvent> where TEvent : IEvent
{
    // would be nice to see a transaction across the event spray
    public async Task Process(
        IMediatorContext context, 
        EventHandlerDelegate next, 
        CancellationToken cancellationToken
    )
    {
        var transaction = hub.StartTransaction("mediator", "event");
        var span = transaction.StartChild(context.MessageHandler.GetType().FullName!);
        
        await next().ConfigureAwait(false);
        foreach (var header in context.Headers)
            span.SetData(header.Key, header.Value);
        
        span.Finish();
        transaction.Finish();
    }
}