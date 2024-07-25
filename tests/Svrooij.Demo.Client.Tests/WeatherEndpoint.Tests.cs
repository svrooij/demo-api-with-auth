﻿using FluentAssertions;
using Microsoft.Kiota.Abstractions;

namespace Svrooij.Demo.Client.Tests;

public class WeatherEndpointTests : IClassFixture<MinimalApiServiceFactory>
{
    private readonly MinimalApiServiceFactory _factory;

    public WeatherEndpointTests(MinimalApiServiceFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetWeather_NoToken_ThrowsException()
    {
        // Arrange
        var demoClient = _factory.CreateApiClient();

        // Act
        Func<Task> act = async () => await demoClient.Weatherforecast.GetAsync();


        // Assert
        var exception = await Assert.ThrowsAsync<ApiException>(act);
        exception.ResponseStatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetWeather_ValidToken_ReturnsClaims()
    {
        // Arrange
        var tokenRequest = new Testcontainers.IdentityProxy.TokenRequest
        {
            Audience = MinimalApiServiceFactory.ACTUAL_AUDIENCE,
            Subject = Guid.NewGuid().ToString(),
            //Issuer = "https://login.microsoftonline.com/mytenant/v2.0",
        };
        var tokenResponse = await _factory.GetTokenAsync(tokenRequest);
        var demoClient = _factory.CreateApiClient(tokenResponse.AccessToken);

        // Act
        var response = await demoClient.Weatherforecast.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response!.Count.Should().Be(5);

        var first = response.First();
        first.Date.Should().Be(new Date(DateTime.Today.AddDays(1)));
    }
}
