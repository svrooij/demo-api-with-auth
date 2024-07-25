using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Svrooij.Demo.Api.Tests;

public class WeatherEndpoint : IClassFixture<MinimalApiServiceFactory>
{
    private readonly MinimalApiServiceFactory _factory;

    public WeatherEndpoint(MinimalApiServiceFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetWeather_NoToken_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWeather_InvalidAudience_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tokenRequest = new Testcontainers.IdentityProxy.TokenRequest { Audience = "xxx", Subject = "yyy" };
        var tokenResult = await _factory.GetTokenAsync(tokenRequest);

        // Act
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokenResult.AccessToken);
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWeather_ValidAudience_ReturnsWeather()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tokenRequest = new Testcontainers.IdentityProxy.TokenRequest { Audience = _factory.Audience!, Subject = "yyy" };
        var tokenResult = await _factory.GetTokenAsync(tokenRequest);

        // Act
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokenResult.AccessToken);
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        response.EnsureSuccessStatusCode();
        var weather = await response.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>();
        weather.Should().HaveCount(5);
        var first = weather!.First();
        first.Date.Should().Be(DateOnly.FromDateTime(DateTime.Now.AddDays(1)));
    }
}
