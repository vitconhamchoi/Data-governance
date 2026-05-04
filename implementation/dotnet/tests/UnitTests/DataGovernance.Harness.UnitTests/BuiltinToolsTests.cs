using DataGovernance.API.Services.Harness.BuiltinTools;
using FluentAssertions;
using System.Text.Json;

namespace DataGovernance.Harness.UnitTests;

public class BuiltinToolsTests
{
    [Fact]
    public async Task EchoTool_Should_Return_Exact_Input()
    {
        var tool = new EchoToolHandler();
        const string payload = "{\"msg\":\"hello\"}";

        var output = await tool.ExecuteAsync(payload, CancellationToken.None);

        output.Should().Be(payload);
    }

    [Fact]
    public async Task UtcNowTool_Should_Return_Valid_Json_With_UtcNow_Field()
    {
        var tool = new UtcNowToolHandler();

        var output = await tool.ExecuteAsync("{}", CancellationToken.None);

        var document = JsonDocument.Parse(output);
        document.RootElement.TryGetProperty("utcNow", out var utcNowProperty).Should().BeTrue();
        utcNowProperty.GetDateTimeOffset().Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
