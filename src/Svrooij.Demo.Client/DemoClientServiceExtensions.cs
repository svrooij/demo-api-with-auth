using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

namespace Svrooij.Demo.Client;
/// <summary>
/// 
/// </summary>
public static class DemoClientServiceExtensions
{
    /// <summary>
    /// Register the DemoClient with the provided options and a static authentication provider
    /// </summary>
    /// <typeparam name="T">IAuthenticationProvider implementation</typeparam>
    /// <param name="services">Service collection you want to use</param>
    /// <param name="configureOptions">Options action, to configure stuff</param>
    /// <param name="authenticationProvider">Your static authentication provider</param>
    /// <returns></returns>
    public static IServiceCollection AddDemoClient<T>(this IServiceCollection services, Action<DemoClientOptions> configureOptions, T? authenticationProvider = default) where T : IAuthenticationProvider
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        if (authenticationProvider is not null)
        {
            services.AddKeyedSingleton<IAuthenticationProvider>(DemoClientFactory.AuthKey, authenticationProvider);
        }
        services.AddDemoClientInternal(configureOptions);
        return services;
    }

    /// <summary>
    /// Register the DemoClient with the provided options and a transient authentication provider
    /// </summary>
    /// <param name="services">Service collection you want to use</param>
    /// <param name="configureOptions">Options action, to configure stuff</param>
    /// <param name="createAuthProvider">A function that is provided with the <see cref="IServiceProvider"/> and should return a <see cref="IAuthenticationProvider"/></param>
    /// <returns></returns>
    public static IServiceCollection AddDemoClient(this IServiceCollection services, Action<DemoClientOptions> configureOptions, Func<IServiceProvider, IAuthenticationProvider> createAuthProvider)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentNullException.ThrowIfNull(createAuthProvider);
        services.AddKeyedTransient<IAuthenticationProvider>(DemoClientFactory.AuthKey, (sp, _) => createAuthProvider(sp));
        services.AddDemoClientInternal(configureOptions);
        return services;
    }

    private static IServiceCollection AddDemoClientInternal(this IServiceCollection services, Action<DemoClientOptions> configureOptions)
    {
        services.Configure(configureOptions);
        // Add the Kiota handlers if they are not already added
        // Please do not register some handlers manually, always register all of them.
        if (!services.Any(s => s.ServiceType == typeof(RetryHandler)))
        {
            services.AddKiotaHandlersForDemoClient();
        }
        services.AddHttpClient<DemoClient>().AttachKiotaHandlersForDemoClient();
        services.AddTransient<DemoClientFactory>();
        services.AddTransient((sp) => sp.GetRequiredService<DemoClientFactory>().CreateClient());
        return services;
    }

    // This method is used to add the Kiota handlers to the DI container
    // It's private by design, as it should not be called from outside this class
    private static IServiceCollection AddKiotaHandlersForDemoClient(this IServiceCollection services)
    {
        // Dynamically load the Kiota handlers from the Client Factory
        var kiotaHandlers = KiotaClientFactory.GetDefaultHandlerTypes();
        // And register them in the DI container
        foreach (var handler in kiotaHandlers)
        {
            services.AddTransient(handler);
        }

        return services;
    }

    // This method is used to attach the Kiota handlers to the HttpClientBuilder
    // It's private by design, as it should not be called from outside this class
    private static IHttpClientBuilder AttachKiotaHandlersForDemoClient(this IHttpClientBuilder builder)
    {
        // Dynamically load the Kiota handlers from the Client Factory
        var kiotaHandlers = KiotaClientFactory.GetDefaultHandlerTypes();
        // And attach them to the http client builder
        foreach (var handler in kiotaHandlers)
        {
            builder.AddHttpMessageHandler((sp) => (DelegatingHandler)sp.GetRequiredService(handler));
        }

        return builder;
    }
}
