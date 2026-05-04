using DataGovernance.API.Configuration;
using DataGovernance.API.Services.Harness;
using DataGovernance.Application.Harness.Abstractions;
using DataGovernance.Application.Harness.Contracts;
using DataGovernance.Domain.Entities.Harness;
using DataGovernance.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace DataGovernance.Harness.UnitTests;

public class ToolHarnessServiceTests
{
    [Fact]
    public async Task InvokeAsync_Should_Reject_When_Caller_Permission_Too_Low()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ToolDefinitions.Add(new ToolDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "secure_tool",
            Description = "secure",
            InputSchema = "{}",
            OutputSchema = "{}",
            PermissionLevel = ToolPermissionLevel.Admin,
            SideEffectLevel = ToolSideEffectLevel.HighRisk,
            TimeoutSeconds = 5,
            RetryCount = 0,
            CreatedBy = "test",
            UpdatedBy = "test"
        });
        await dbContext.SaveChangesAsync();

        var handler = new Mock<IToolHandler>();
        handler.Setup(x => x.Name).Returns("secure_tool");
        handler.Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("{}");

        var service = new ToolHarnessService(
            dbContext,
            new[] { handler.Object },
            Options.Create(new HarnessOptions()));

        var result = await service.InvokeAsync(
            new ToolInvocationRequest(
                "secure_tool",
                "{}",
                new ToolInvocationContext(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Guid.NewGuid(),
                    null,
                    ToolPermissionLevel.Public)),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("permission_denied");
        handler.Verify(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static DataGovernanceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DataGovernanceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataGovernanceDbContext(options);
    }
}
