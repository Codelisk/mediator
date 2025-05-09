using System.Runtime.CompilerServices;

namespace Shiny.Mediator.Infrastructure;


public class SentryStreamRequestMiddleware<TRequest, TResult>(IHub hub) : IStreamRequestMiddleware<TRequest, TResult> 
    where TRequest : IStreamRequest<TResult>
{
    public async IAsyncEnumerable<TResult> Process(
        IMediatorContext context, 
        StreamRequestHandlerDelegate<TResult> next,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var transaction = hub.StartTransaction("mediator", "stream");
        var span = transaction.StartChild(context.MessageHandler.GetType().FullName!);
        var nxt = next().GetAsyncEnumerator(cancellationToken);
        
        var requestKey = ContractUtils.GetRequestKey(context.Message!);
        span.SetData("RequestKey", requestKey);
        
        var moveSpan = span.StartChild("initial_movenext");
        while (await nxt.MoveNextAsync() && !cancellationToken.IsCancellationRequested)
        {
            yield return nxt.Current;
            moveSpan.Finish();
            moveSpan = span.StartChild("movenext");
        }
        span.Finish();
        transaction.Finish();
    }
}