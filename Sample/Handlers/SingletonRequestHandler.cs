using Sample.Contracts;

namespace Sample.Handlers;


[SingletonHandler]
[TimedLogging(3000)]
public class SingletonRequestHandler(IMediator mediator, AppSqliteConnection data) : IRequestHandler<MyMessageRequest, MyMessageResponse>
{
    // [Cache(Storage = StoreType.File, MaxAgeSeconds = 30, OnlyForOffline = true)]
    [OfflineAvailable]
    public async Task<MyMessageResponse> Handle(MyMessageRequest request, CancellationToken cancellationToken)
    {
        var e = new MyMessageEvent(
            request.Arg,
            request.FireAndForgetEvents
        );
        await data.Log("SingletonRequestHandler", e);
        if (request.FireAndForgetEvents)
        {
            mediator.Publish(e).RunInBackground(ex =>
            {
                // log this or something
                Console.WriteLine(ex);
            });
        }
        else
        {
            await mediator.Publish(
                e,
                cancellationToken
            );
        }

        return new MyMessageResponse("RESPONSE: " + request.Arg);
    }
}