using DataGovernance.Application.Harness.Abstractions;
using DataGovernance.Application.Harness.Contracts;
using DataGovernance.Domain.Entities.Harness;
using DataGovernance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Text.RegularExpressions;

namespace DataGovernance.API.Services.Harness;

public sealed class AIHarnessService : IAIHarnessService
{
    private readonly DataGovernanceDbContext _dbContext;
    private readonly IChatClientFactory _chatClientFactory;
    private readonly IToolHarnessService _toolHarnessService;
    private readonly ILogger<AIHarnessService> _logger;

    public AIHarnessService(
        DataGovernanceDbContext dbContext,
        IChatClientFactory chatClientFactory,
        IToolHarnessService toolHarnessService,
        ILogger<AIHarnessService> logger)
    {
        _dbContext = dbContext;
        _chatClientFactory = chatClientFactory;
        _toolHarnessService = toolHarnessService;
        _logger = logger;
    }

    public async Task<RunDto> CreateRunAsync(CreateRunRequest request, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(x => x.Id == request.SessionId && x.TenantId == request.TenantId, cancellationToken);

        if (session is null)
        {
            session = new Session
            {
                Id = request.SessionId,
                TenantId = request.TenantId,
                Channel = "api",
                Title = "AI Harness Session",
                CreatedBy = "system",
                UpdatedBy = "system"
            };
            _dbContext.Sessions.Add(session);
        }

        var userMessage = new Message
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            SessionId = request.SessionId,
            Role = MessageRole.User,
            Content = request.Input,
            CreatedBy = "user",
            UpdatedBy = "user"
        };

        var run = new Run
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            SessionId = request.SessionId,
            Input = request.Input,
            TaskType = request.TaskType ?? "general",
            Strategy = request.Strategy ?? "default",
            Model = request.Model,
            Provider = request.Provider,
            StartedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
            UpdatedBy = "system"
        };

        _dbContext.Messages.Add(userMessage);
        _dbContext.Runs.Add(run);

        AddStep(run, 1, "create_run", null, RunStepStatus.Completed, request.Input, "run_created", null);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(run);
    }

    public async Task<RunDto?> GetRunAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken)
    {
        var run = await _dbContext.Runs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == runId && x.TenantId == tenantId, cancellationToken);

        return run is null ? null : ToDto(run);
    }

    public async Task<IReadOnlyList<RunStepDto>> GetRunStepsAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken)
    {
        return await _dbContext.RunSteps
            .AsNoTracking()
            .Where(x => x.RunId == runId && x.TenantId == tenantId)
            .OrderBy(x => x.StepNo)
            .Select(x => new RunStepDto(
                x.Id,
                x.RunId,
                x.StepNo,
                x.StepType,
                x.ToolName,
                x.Status,
                x.Input,
                x.Output,
                x.ErrorCode,
                x.ErrorMessage,
                x.StartedAt,
                x.CompletedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task ProcessRunAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken)
    {
        var run = await _dbContext.Runs
            .FirstOrDefaultAsync(x => x.Id == runId && x.TenantId == tenantId, cancellationToken);

        if (run is null)
        {
            return;
        }

        if (run.Status is RunStatus.Completed or RunStatus.Failed or RunStatus.Cancelled)
        {
            return;
        }

        var stepNo = await _dbContext.RunSteps.CountAsync(x => x.RunId == run.Id, cancellationToken) + 1;
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            TransitionOrThrow(run, RunStatus.Classifying);
            AddStep(run, stepNo++, "classify", null, RunStepStatus.Completed, run.Input, "general", null);

            TransitionOrThrow(run, RunStatus.Planning);
            AddStep(run, stepNo++, "plan", null, RunStepStatus.Completed, run.Input, "single-step execution", null);

            if (RequiresManualApproval(run.Input))
            {
                TransitionOrThrow(run, RunStatus.AwaitingApproval);
                _dbContext.Approvals.Add(new Approval
                {
                    Id = Guid.NewGuid(),
                    TenantId = run.TenantId,
                    RunId = run.Id,
                    ActionType = "manual_confirmation",
                    RequestedBy = "system",
                    Status = ApprovalStatus.Requested,
                    CreatedBy = "system",
                    UpdatedBy = "system"
                });
                AddStep(run, stepNo, "approval", null, RunStepStatus.Pending, run.Input, null, null);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            TransitionOrThrow(run, RunStatus.Executing);

            string executionOutput;
            if (TryParseToolInvocation(run.Input, out var toolName, out var payload))
            {
                var toolResult = await _toolHarnessService.InvokeAsync(
                    new ToolInvocationRequest(
                        toolName,
                        payload,
                        new ToolInvocationContext(run.TenantId, run.Id, null, ToolPermissionLevel.Admin)),
                    cancellationToken);

                if (!toolResult.Success)
                {
                    throw new InvalidOperationException($"Tool call failed: {toolResult.ErrorCode} - {toolResult.ErrorMessage}");
                }

                executionOutput = toolResult.OutputJson;
                AddStep(run, stepNo++, "tool_execution", toolName, RunStepStatus.Completed, payload, executionOutput, null);
            }
            else
            {
                var chatClient = _chatClientFactory.CreateChatClient(run.Provider, run.Model);

                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, "You are an AI harness executor. Respond concisely."),
                    new(ChatRole.User, run.Input)
                };

                var response = await chatClient.CompleteAsync(messages, cancellationToken: cancellationToken);
                executionOutput = response.Message.Text ?? string.Empty;

                AddStep(run, stepNo++, "model_execution", null, RunStepStatus.Completed, run.Input, executionOutput, null);
            }

            TransitionOrThrow(run, RunStatus.Synthesizing);
            run.FinalOutput = executionOutput;
            AddStep(run, stepNo++, "synthesize", null, RunStepStatus.Completed, executionOutput, executionOutput, null);

            TransitionOrThrow(run, RunStatus.Completed);
            run.LatencyMs = (int)(DateTimeOffset.UtcNow - startTime).TotalMilliseconds;

            _dbContext.Messages.Add(new Message
            {
                Id = Guid.NewGuid(),
                TenantId = run.TenantId,
                SessionId = run.SessionId,
                Role = MessageRole.Assistant,
                Content = run.FinalOutput ?? string.Empty,
                CreatedBy = "assistant",
                UpdatedBy = "assistant"
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Run {RunId} failed", run.Id);
            run.LastError = ex.Message;
            run.SetStatusUnsafe(RunStatus.Failed);

            AddStep(run, stepNo, "failure", null, RunStepStatus.Failed, run.Input, null, ex.Message);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ResumeInFlightRunsAsync(CancellationToken cancellationToken)
    {
        var resumableStatuses = new[]
        {
            RunStatus.Pending,
            RunStatus.Classifying,
            RunStatus.Planning,
            RunStatus.Executing,
            RunStatus.WaitingExternal,
            RunStatus.Synthesizing
        };

        var runs = await _dbContext.Runs
            .Where(x => resumableStatuses.Contains(x.Status))
            .Select(x => new { x.Id, x.TenantId })
            .ToListAsync(cancellationToken);

        foreach (var run in runs)
        {
            await ProcessRunAsync(run.TenantId, run.Id, cancellationToken);
        }
    }

    private static RunDto ToDto(Run run)
    {
        return new RunDto(
            run.Id,
            run.TenantId,
            run.SessionId,
            run.Status,
            run.Input,
            run.FinalOutput,
            run.Model,
            run.Provider,
            run.TaskType,
            run.Strategy,
            run.StartedAt,
            run.CompletedAt,
            run.PromptTokens,
            run.CompletionTokens,
            run.CostUsd,
            run.LastError);
    }

    private static void TransitionOrThrow(Run run, RunStatus newStatus)
    {
        if (!run.TryTransitionTo(newStatus, out var error))
        {
            throw new InvalidOperationException(error);
        }
    }

    private static bool RequiresManualApproval(string input)
    {
        return Regex.IsMatch(input, @"\bapprove\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool TryParseToolInvocation(string input, out string toolName, out string payload)
    {
        toolName = string.Empty;
        payload = "{}";

        var match = Regex.Match(
            input.Trim(),
            @"^tool:(?<name>[a-zA-Z0-9_-]+)(?:\s+(?<payload>.+))?$",
            RegexOptions.CultureInvariant);

        if (!match.Success)
        {
            return false;
        }

        toolName = match.Groups["name"].Value;
        payload = match.Groups["payload"].Success ? match.Groups["payload"].Value : "{}";
        return true;
    }

    private void AddStep(
        Run run,
        int stepNo,
        string stepType,
        string? toolName,
        RunStepStatus status,
        string? input,
        string? output,
        string? error)
    {
        _dbContext.RunSteps.Add(new RunStep
        {
            Id = Guid.NewGuid(),
            TenantId = run.TenantId,
            RunId = run.Id,
            StepNo = stepNo,
            StepType = stepType,
            ToolName = toolName,
            Status = status,
            Input = input,
            Output = output,
            ErrorMessage = error,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
            UpdatedBy = "system"
        });
    }
}
