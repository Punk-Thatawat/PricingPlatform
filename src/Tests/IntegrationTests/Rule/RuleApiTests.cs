using System.Net;
using System.Net.Http.Json;
using Rule.Application.DTOs;
using Rule.Domain.Enums;

namespace PricingPlatform.IntegrationTests.Rule;

public sealed class RuleApiTests : IClassFixture<RuleApiFactory>
{
    private readonly HttpClient _client;

    public RuleApiTests(RuleApiFactory factory)
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
    public async Task CreateRule_ThenListActiveRules_ReturnsCreatedRule()
    {
        var request = new CreateRuleRequestDto
        {
            Type = RuleType.WeightTier,
            IsActive = true,
            ConfigJson = "{\"tiers\":[{\"minWeight\":0,\"maxWeight\":5,\"pricingType\":0,\"value\":20}]}",
            Priority = 1,
            EffectiveFrom = DateTime.UtcNow
        };

        var createResponse = await _client.PostAsJsonAsync("/rules", request);
        var created = await createResponse.Content.ReadFromJsonAsync<RuleDto>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(created);

        var activateResponse = await _client.PatchAsync($"/rules/{created.Id}/activate", null);
        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);

        var listResponse = await _client.GetAsync("/rules?isActive=true");
        var rules = await listResponse.Content.ReadFromJsonAsync<RuleDto[]>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(rules);
        Assert.Contains(rules, rule => rule.Id == created.Id);
    }
}
