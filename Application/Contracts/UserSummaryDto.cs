namespace FlowGate.Core.Application.Contracts;

public sealed record UserSummaryDto(
    Guid Id,
    string DisplayName,
    string Email,
    bool IsActive,
    IReadOnlyCollection<string> Roles);