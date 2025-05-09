using Sample.Contracts;

namespace Sample.Handlers;


[SingletonHandler]
public class ErrorCommandHandler : ICommandHandler<ErrorCommand>
{
    public Task Handle(ErrorCommand command, IMediatorContext context, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Why you call me?");
    }
}