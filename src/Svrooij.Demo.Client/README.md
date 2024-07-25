# DemoClient

This project has the auto-generated Kiota client for the demo API. It's a strongly typed client that is generated from the OpenAPI specification that is generated at build time.

The client is generated with the following command:

```bash
# You need to have the kiota cli installed
# dotnet tool install -g Microsoft.OpenAPI.Kiota
kiota generate --ll Error -l CSharp -c DemoClient -n Svrooij.Demo.Client -d ../Svrooij.Demo.Api/wwwroot/swagger/v1/swagger.json -o ./Generated --serializer Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory --deserializer Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory
```

As you see it does need the OpenAPI specification, but luckily that is also [generated at build time](https://github.com/svrooij/demo-api-with-auth?tab=readme-ov-file#generate-openapi-specification).

## Auto generation?

But you'll never have to run this command, because it will be run as soon as you open the project in Visual Studio or VS Code. Or when you call `dotnet restore` on either the solution or the project.

This command it run by the following code in the [Svrooij.Demo.Client.csproj](./Svrooij.Demo.Client.csproj) file:

```xml
  <Target Name="GenerateRestClient" DependsOnTargets="CleanGenerateRestClient;AutoGenerateRestClient" />
  <Target Name="CleanGenerateRestClient" AfterTargets="CoreClean">
    <RemoveDir Directories="Generated" />
  </Target>

  <Target Name="AutoGenerateRestClient" BeforeTargets="CollectPackageReferences" Outputs="Generated/DemoClient.cs">
    <Message Text="Genering REST Client" Importance="High" Condition="!Exists('./Generated/DemoClient.cs')" />
	<!-- Can we check if the tool is already restored, that would be even better!-->
	<Exec Command="dotnet tool restore -v q" Condition="!Exists('./Generated/DemoClient.cs')" />
	<Exec Command="kiota generate --ll Error -l CSharp -c DemoClient -n Svrooij.Demo.Client -d ../Svrooij.Demo.Api/wwwroot/swagger/v1/swagger.json -o ./Generated --serializer Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory --deserializer Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory" Condition="!Exists('./Generated/DemoClient.cs')" />
	<OnError ExecuteTargets="ClientGenerationError" />
  </Target>

  <Target Name="ClientGenerationError">
	<Error Text="DemoClient could not be generated" />
  </Target>
```

This code uses the `CollectPackageReferences` target to run the `AutoGenerateRestClient` target before the project is loaded. This way the client is always generated when you open the project.

## Usage

To use the client in your project, you have to do a project reference to this project, in a not demo project this would be a NuGet package.
After you added to reference, you can add it to your dependency injection container like this:

```csharp
// If you have a singleton authentication provider, register it like this
builder.Services.AddDemoClient((config) => { config.BaseUrl = new Uri("https://localhost:5001"); }, someIAuthenticationProviderInstance);

// If you have a transient authentication provider, register it like this
builder.Services.AddDemoClient((config) => { config.BaseUrl = new Uri("https://localhost:5001"); }, (sp) => new SomeIAuthenticationProviderImplementation());

// And then you can use it in your controller like this
public class WeatherForecastController : ControllerBase {
	private readonly DemoClient _client;

	public WeatherForecastController(DemoClient client) {
		_client = client;
	}

	[HttpGet]
	public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken) {
		return await _client.Weatherforecast.GetAsync(cancellationToken: cancellationToken);
	}
}
```
