namespace DataGovernance.Domain.Entities.Harness;

public enum SessionStatus
{
    Active = 0,
    Archived = 1,
    Closed = 2
}

public enum MessageRole
{
    System = 0,
    User = 1,
    Assistant = 2,
    Tool = 3
}

public enum RunStatus
{
    Pending = 0,
    Classifying = 1,
    Planning = 2,
    AwaitingApproval = 3,
    Executing = 4,
    WaitingExternal = 5,
    Synthesizing = 6,
    Completed = 7,
    Failed = 8,
    Cancelled = 9
}

public enum RunStepStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Skipped = 4
}

public enum ApprovalStatus
{
    Requested = 0,
    Approved = 1,
    Denied = 2,
    Expired = 3
}

public enum MemoryType
{
    Conversation = 0,
    Working = 1,
    LongTerm = 2
}

public enum ToolSideEffectLevel
{
    Read = 0,
    Write = 1,
    HighRisk = 2
}

public enum ToolPermissionLevel
{
    Public = 0,
    Restricted = 1,
    Admin = 2
}
