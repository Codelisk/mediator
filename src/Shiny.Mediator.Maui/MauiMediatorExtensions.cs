using Shiny.Mediator.Middleware;

namespace Shiny.Mediator;


public class MauiServiceProvider : IMauiInitializeService
{
    public static IServiceProvider Services { get; private set; } = null!;
    public void Initialize(IServiceProvider services)
    {
        Services = services;
    }
}


public static class MauiMediatorExtensions
{
    public static ShinyConfigurator UseMaui(this ShinyConfigurator cfg) => cfg.AddEventCollector<MauiEventCollector>();

    public static ShinyConfigurator AddTimedMiddleware(this ShinyConfigurator cfg, TimedLoggingMiddlewareConfig config)
    {
        cfg.Services.AddSingleton(config);
        return cfg.AddOpenRequestMiddleware(typeof(TimedLoggingRequestMiddleware<,>));
    }


    public static ShinyConfigurator AddEventExceptionHandling(this ShinyConfigurator cfg)
    {
        cfg.AddOpenEventMiddleware(typeof(ExceptionHandlerEventMiddleware<>));
        return cfg;
    }
    

    public static ShinyConfigurator AddMainThreadEventMiddleware(this ShinyConfigurator cfg)
    {
        cfg.AddOpenEventMiddleware(typeof(MainTheadEventMiddleware<>));
        return cfg;
    }
    
    
    public static ShinyConfigurator AddCacheMiddleware(this ShinyConfigurator cfg)
    {
        cfg.AddOpenRequestMiddleware(typeof(CacheRequestMiddleware<,>));
        return cfg;
    }
    
    
    public static ShinyConfigurator AddUserNotificationExceptionMiddleware(this ShinyConfigurator cfg, UserExceptionRequestMiddlewareConfig config)
    {
        cfg.Services.AddSingleton(config);
        cfg.AddOpenRequestMiddleware(typeof(UserExceptionRequestMiddleware<,>));
        return cfg;
    }
}