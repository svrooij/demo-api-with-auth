using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.IdentityProxy;

namespace Svrooij.Demo.Api.Tests;

/// <summary>
/// This class will be injected into our tests to create a new instance of our API.
/// </summary>
/// <remarks>To get this to work with a minimal api, you'll need to add `public partial class Program { }` to your Program.cs file. Tricking it in making the class Program public</remarks>
public class MinimalApiServiceFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Configuration for the Identity Proxy, we will be using to mock entra id.
    private const string ACTUAL_AUTHORITY = "https://login.microsoftonline.com/common/v2.0";
    private readonly IdentityProxyContainer _identityProxy = new IdentityProxyBuilder().WithAuthority(ACTUAL_AUTHORITY).Build();

    internal string? Audience { get; private set; } = null; 

    // This method is called before the test is run, and will allow us to change the configuration of the webhost.
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        // Change the authority to the identity proxy
        builder.UseSetting("JWT:Authority", _identityProxy.GetAuthority());
        // JWT middleware does require https, unless you explicitly tell it not too.
        // which is why you normally would set `options.RequireHttpsMetadata = true;` in the AddJwtBearer method.
        builder.UseSetting("JWT:RequireHttpsMetadata", "false");

        // This is a "hack" to get the audience from the configuration.
        builder.ConfigureAppConfiguration((context, config) =>
        {
            Audience = context.Configuration["JWT:TokenValidationParameters:ValidAudiences:0"];
        });

    }

    // Expose the token request method
    internal Task<TokenResult> GetTokenAsync(TokenRequest tokenRequest)
    {
        return _identityProxy.GetTokenAsync(tokenRequest);
    }

    public async Task InitializeAsync()
    {
        await _identityProxy.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _identityProxy.DisposeAsync();
    }
}
