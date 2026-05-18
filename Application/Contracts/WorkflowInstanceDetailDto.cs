using System.Text.Json;

namespace FlowGate.Core.Application.Contracts;

public sealed record WorkflowInstanceDetailDto(
    Guid Id,
    Guid WorkflowId,
    string WorkflowKey,
    string WorkflowName,
    string Title,
    string CurrentStatus,
    Guid RequesterId,
    string RequesterName,
    Dictionary<string, string>? FormData,
    IReadOnlyList<ApprovalStepDetailDto> Steps,
    IReadOnlyList<AuditLogEntryDto> AuditLog,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? CompletedAtUtc);

public sealed record ApprovalStepDetailDto(
    Guid Id,
    int StepIndex,
    string? GroupId,
    string Name,
    string RequiredRole,
    string Status,
    string? AssignedUserName,
    string? Comment,
    DateTimeOffset? DecidedAtUtc);

public sealed record AuditLogEntryDto(
    Guid Id,
    string Action,
    string PerformedByName,
    string? Comment,
    DateTimeOffset PerformedAtUtc);
