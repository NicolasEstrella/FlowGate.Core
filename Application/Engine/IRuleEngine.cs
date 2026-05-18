using FlowGate.Core.Domain.Enums;

namespace FlowGate.Core.Application.Engine;

public record RuleContext(
    string ProcessType,
    decimal Amount,
    string Currency,
    IReadOnlyDictionary<string, string> ExtraFields);

public record ApprovalStepDefinition(
    int StepIndex,
    string Name,
    string RequiredRole,
    StepExecutionType ExecutionType,
    string? GroupId);

public interface IRuleEngine
{
    IReadOnlyList<ApprovalStepDefinition> Evaluate(RuleContext context);
}
