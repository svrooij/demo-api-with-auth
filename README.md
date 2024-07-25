# Demo API

This is a sample project that displays all the cool features I love about DOTNET. Including:

- [x] Minimal API
- [x] Token Authentication
- [x] OpenAPI Documentation [generated at build time](#generate-openapi-specification)
- [x] Swagger UI Documentation (with working token authentication)
- [x] Tests that actual test the API [test the api](#testing-the-api)
- [x] Strongly typed API client using Kiota [generated at build time](#kiota)

Follow along on [LinkedIn](https://www.linkedin.com/posts/stephanvanrooij_github-svrooijdemo-api-with-auth-a-demo-activity-7222324418478325760-2SGI?utm_source=share&utm_medium=member_desktop) for more updates.
And while you're at it, let me know in the post what you think.

## Hosted DEMO

This project will be hosted on Azure, you can check it out here....

## Getting started yourself

The API is protected with a tokens from Entra ID. I got an API and a client app registered to get you started, but you'll need to do some setup yourself if you want to try this with your own app.

1. Clone the repository
1. Create [Entra ID API registration](#entra-id-api-registration)]
1. Create [Entra ID client registration](#entra-id-client-registration)
1. Fill in the [appsettings.json](#appsettings)

### Entra ID API registration

1. Go to [Entra ID app registrations](https://entra.microsoft.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade/quickStartType//sourceType/Microsoft_AAD_IAM) and click new registration.
1. Give your app a name `Stephan' demo API`
1. Select the account types that can use your app `Accounts in any organizational directory and personal Microsoft accounts`
1. Skip the redirect URI
1. Click register
1. Click Expose an API and click on the `Add` button next to `Application ID URI`
1. Leave the default URI and click save (you need this value in your appsettings.json)
1. Click on `Add a scope`, Scope name: `access_as_user`, Who can consent `Admins and Users`, Pick a display name and description.

### Entra ID client registration

1. Go to [Entra ID app registrations](https://entra.microsoft.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade/quickStartType//sourceType/Microsoft_AAD_IAM) and click new registration.
1. Give your app a name `Stephan' demo API client`
1. Select the account types that can use your app `Accounts in any organizational directory and personal Microsoft accounts`
1. Select `Single-page application` and enter the redirect URI `https://localhost:8080/swagger/oauth2-redirect.html` (the port won't matter, for localhost)
1. Click register
1. Go to the API permissions and add the API you created in the previous step, select the scope you created in the previous step.
1. Copy the `Application (client) ID`, go to the app registration from the previous step and add the client id under `Expose an API` -> `Authorized client applications`

### Appsettings

Open the `appsettings.json` file in your project and fill the folling values:

- `JWT:TokenValidationParameters:ValidAudience` with the `Application ID URI` from the API registration, and the Application id (without the `api://` part). Do NOT change the order of the values!
- `Swagger:ClientId` with the `Application (client) ID` from the client registration.
- `Swagger:Scope` with the scope you created in the API registration (if you picked another name).

## Testing the API

Since we now have an api that is protected with tokens from Entra ID, we need a way to get those tokens from our test project. Preferably without creating credentials in Entra ID itself, and having to manage those tokens in our test project.

This is where [IdentityProxy](https://github.com/svrooij/identityproxy/) comes in. It's a simple project that runs a webservice in a docker container which we configure to emulate tokens as if they came from Entra ID. More details can be found in [this blog post](https://svrooij.io/2024/07/10/integration-tests-protected-api/).

### Svrooij.Demo.Api.Tests

In the test project you'll find a `MinimalApiServiceFactory` this is using the `WebApplicationFactory` from the `Microsoft.AspNetCore.Mvc.Testing` package to start the API in a test environment. It also starts the `IdentityProxy` in a docker container, and configures the API to use the proxy as the token issuer.
This class is then injected in the `WeatherEndpoint.Tests` class to test the API.

The factory can be used to get tokens and to create a `HttpClient` that is configured for the API.

You are even able to *debug* the actual endpoint (only the success path) by setting a breakpoint in the `WeatherEndpoint` class and debugging the test. The unsuccessful path is also tested, but since those responses come from the Jwt middleware, you can't debug those.

## Generate OpenAPI specification

Most solutions generate the OpenAPI specification at **runtime**, but I prefer to generate them at **compile time** and to serve a static version. It won't change dynamically, but we are not using any dynamic endpoints anyway.

The generated version can be found at `wwwroot/swagger/v1/swagger.json` and is served by the `app.UseStaticFiles()` middleware.`

How does it work? Thanks to [this page](https://khalidabuhakmeh.com/generate-aspnet-core-openapi-spec-at-build-time) I got a great idea on how to make it work.
In the [src/Svrooij.Demo.Api](./src/Svrooij.Demo.Api/Svrooij.Demo.Api.csproj) project file there is a new build target that runs the `SwashBuckle.AspNetCore.Cli` to generate the OpenAPI specification.

There also is a target that restores the required tools after `restore`, making sure the tools is available when the build target runs.

> And if you happen to call `dotnet restore` on the solution, it will also restore the tools for you, using the [after.{Solution}.sln.targets](./after.Svrooij.Demo.sln.targets) file.

## Kiota

The Kiota CLI is configured to generate a strongly typed client for the API. The client is generated in the [src/Svrooij.Demo.Client](./src/Svrooij.Demo.Client) project.
It's generated in the `Generated` folder, using some cleaver msbuild tricks, the main ingredient is the `CollectPackageReferences` that is used as `BeforeTargets`.
Meaning this target is run at the very moment the project is loaded.

The `Generated` folder is also added to the `.gitignore` file, so the client is always freshly generated.
And if you want to regenerate the client, you can just delete the folder and within seconds it's back.
The folder is also deleted when you call `clean`.

In the client project, you will also find some handy extension methods to use the Kiota generated client with [dependency injection](https://svrooij.io/2024/07/03/kiota-dependency-injection/).

## Contributing

Want to contribute something to this demo project? Let's make it even better! Or lets [discuss](https://github.com/svrooij/demo-api-with-auth/discussions) improvements.
