using DataGovernance.Domain.Entities.Harness;

namespace DataGovernance.Application.Harness.Contracts;

public sealed record CreateRunRequest(
    Guid TenantId,
    Guid SessionId,
    string Input,
    string Model,
    string Provider,
    string? TaskType = null,
    string? Strategy = null);

public sealed record RunDto(
    Guid Id,
    Guid TenantId,
    Guid SessionId,
    RunStatus Status,
    string Input,
    string? FinalOutput,
    string Model,
    string Provider,
    string TaskType,
    string Strategy,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    int PromptTokens,
    int CompletionTokens,
    decimal CostUsd,
    string? LastError);

public sealed record RunStepDto(
    Guid Id,
    Guid RunId,
    int StepNo,
    string StepType,
    string? ToolName,
    RunStepStatus Status,
    string? Input,
    string? Output,
    string? ErrorCode,
    string? ErrorMessage,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);

public sealed record RegisterToolRequest(
    Guid TenantId,
    string Name,
    string Description,
    string InputSchema,
    string OutputSchema,
    int TimeoutSeconds,
    int RetryCount,
    ToolPermissionLevel PermissionLevel,
    ToolSideEffectLevel SideEffectLevel);

public sealed record ToolInvocationContext(
    Guid TenantId,
    Guid RunId,
    Guid? UserId,
    ToolPermissionLevel CallerPermissionLevel);

public sealed record ToolInvocationRequest(
    string ToolName,
    string InputJson,
    ToolInvocationContext Context);

public sealed record ToolInvocationResult(
    bool Success,
    string OutputJson,
    string? ErrorCode,
    string? ErrorMessage,
    TimeSpan Duration);
