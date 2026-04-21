using System.Net;
using System.Net.Http.Json;
using Pricing.Application.DTOs;

namespace PricingPlatform.IntegrationTests.Pricing;

public sealed class PricingApiTests : IClassFixture<PricingApiFactory>
{
    private readonly HttpClient _client;

    public PricingApiTests(PricingApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPing_ReturnsPong()
    {
        var response = await _client.GetAsync("/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostPrice_ReturnsCalculatedQuote()
    {
        var request = new PriceQuoteRequestDto
        {
            BasePrice = 100m,
            Weight = 5m,
            Zone = "A",
            HourOfDay = 9,
            DayOfWeek = DayOfWeek.Monday
        };

        var response = await _client.PostAsJsonAsync("/quotes/price", request);
        var payload = await response.Content.ReadFromJsonAsync<PriceQuoteResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(115m, payload.FinalPrice);
        Assert.Equal(1, payload.AppliedRuleCount);
    }
}
