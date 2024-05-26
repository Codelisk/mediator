namespace Sample.Handlers.MyMessage;

public class SingletonEventHandler(IMediator mediator) : IEventHandler<MyMessageEvent>
{
    public async Task Handle(MyMessageEvent @event, CancellationToken cancellationToken)
    {
        var random = new Random();
        var wait = random.Next(500, 5000);
        await Task.Delay(wait, cancellationToken);
        
        Console.WriteLine("SingletonEventHandler: " + @event.Arg);
    }
}