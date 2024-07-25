using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Svrooij.Demo.Api;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var securityName = "Bearer";
// The default scope is build up from the first audience in the JWT section and the Scope in the swagger section.
// And will look like `api://<client-id>/<scope>` (if you did not change the default value when creating your api registration in Entra ID).
var defaultScope = $"{builder.Configuration.GetValue<string>("JWT:TokenValidationParameters:ValidAudiences:0")}/{builder.Configuration.GetValue<string>("Swagger:Scope")}";
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    swagger.AddSecurityDefinition(securityName, new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.OAuth2,
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        Name = "Authorization",
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/authorize", UriKind.Absolute),
                TokenUrl = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/token", UriKind.Absolute),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "Default" },
                    { defaultScope, "API Access" }
                }
            }
        }
    });
});

builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Bind the `JWT` section of the appsettings.json file to the options
        // Meaning we can now configure how the JWT should be validated in the appsettings.json file
        builder.Configuration.Bind("JWT", options);
        // Normally you would want to require https, no matter what the configuration says (security tip!!)
        // but to be able to easily override this in the tests, we will not set it here.
        //options.RequireHttpsMetadata = true;
        // Add settings you don't want to be configurable in the appsettings.json file here
        options.TokenValidationParameters.RequireAudience = true;
        options.TokenValidationParameters.RequireExpirationTime = true;
        options.TokenValidationParameters.RequireSignedTokens = true;
        // Don't validate the issuer, we want to have a multi-tenant application
        options.TokenValidationParameters.ValidateIssuer = false;
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
    });

builder.Services.AddAuthorization(options =>
{
    // Add a default policy that requires the user to be authenticated and have a `NameIdentifier` claim
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(ClaimTypes.NameIdentifier)
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(swagger =>
    {
        // This will pre-fill the client id for easier login
        swagger.OAuthClientId(builder.Configuration.GetValue<string>("Swagger:ClientId"));
        swagger.OAuthAppName("Stephans' Demo API Client");
        // Turn on PKCE for the swagger UI (single-page applications should always use PKCE)
        swagger.OAuthUsePkce();
        // Separate the scopes with a space
        swagger.OAuthScopeSeparator(" ");
        swagger.EnableTryItOutByDefault();
        swagger.EnableDeepLinking();
        swagger.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
// Add the authorization requirement to the endpoint
.RequireAuthorization()
// Add the OpenAPI metadata to the endpoint
.WithOpenApi(operation =>
{
    operation.Summary = "Get the weather forecast";
    operation.Description = "Get the weather forecast for the next 5 days";
    operation.Tags = [new OpenApiTag { Name = "Weather" }];
    // Add the security requirement to the endpoint, (this will make the lock icon appear in the swagger UI, on this endpoint)
    operation.AddSecurityRequirement(securityName, defaultScope, "openid");
    return operation;
});

// Add a new endpoint that returns the claims of the user
// Thanks to https://stackoverflow.com/a/74366192/639153
app.MapGet("/claims", (ClaimsPrincipal user) =>
{
    var claims = new List<string>();
    foreach (var claim in user.Claims)
    {
        claims.Add($"{claim.Type}: {claim.Value}");
    }
    return claims;
})
.WithName("GetClaims")
.RequireAuthorization()
.WithOpenApi(operation =>
{
    operation.Summary = "Get the claims of the user";
    operation.Description = "Get the claims of the user that is currently logged in";
    operation.Tags = [new OpenApiTag { Name = "User" }];
    operation.AddSecurityRequirement(securityName, defaultScope, "openid");
    return operation;
});

app.Run();

// Add this to your Program.cs file to make the class public
public partial class Program { }

// This is the model that will be returned by the weather endpoint
// We want to use it in the tests, so we need to make it public
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
