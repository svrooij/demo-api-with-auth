# Demo API

This is a sample project that displays all the cool features I love about DOTNET. Including:

- Minimal API
- Token Authentication
- OpenAPI Documentation
- Swagger UI Documentation (with working token authentication)
- Tests that actual test the API
- Strongly typed Kiota client (that is generated automatically, if you installed kiota on your computer)

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
