using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Svrooij.Demo.Client;

public class DemoClientFactory
{
    internal const string AuthKey = $"{nameof(DemoClient)}Auth";
    private readonly HttpClient _httpClient;
    private readonly DemoClientOptions? _options;
    private readonly IAuthenticationProvider? _authenticationProvider;

    public DemoClientFactory(HttpClient httpClient, IOptions<DemoClientOptions>? options = null, [FromKeyedServices(AuthKey)]IAuthenticationProvider? authenticationProvider = null)
    {
        _httpClient = httpClient;
        _options = options?.Value;
        _httpClient.BaseAddress ??= _options?.BaseUri;
        _authenticationProvider = authenticationProvider;
    }

    public DemoClient CreateClient(string? token = null)
    {
        if (_authenticationProvider is null && string.IsNullOrEmpty(token))
            throw new InvalidOperationException("No authentication provider or token provided");

        IAuthenticationProvider provider = string.IsNullOrEmpty(token) ? _authenticationProvider! : new BaseBearerTokenAuthenticationProvider(new StaticAccessTokenProvider(token!));

        return new DemoClient(new HttpClientRequestAdapter(provider, httpClient: _httpClient));

    }
}
