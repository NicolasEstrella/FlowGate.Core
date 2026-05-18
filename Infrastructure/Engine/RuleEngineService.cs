using FlowGate.Core.Application.Engine;
using FlowGate.Core.Application.Security;
using FlowGate.Core.Domain.Enums;

namespace FlowGate.Core.Infrastructure.Engine;

public sealed class RuleEngineService : IRuleEngine
{
    private const string FinanceLegalGroupId = "finance-legal";

    public IReadOnlyList<ApprovalStepDefinition> Evaluate(RuleContext context)
    {
        var steps = new List<ApprovalStepDefinition>();
        var index = 0;

        steps.Add(new ApprovalStepDefinition(
            index++,
            "ManagerApproval",
            FlowGateRoles.Approver,
            StepExecutionType.Sequential,
            null));

        if (context.Amount > 5_000m)
        {
            bool isContract = string.Equals(context.ProcessType, "contract", StringComparison.OrdinalIgnoreCase);
            var financeGroupId = isContract ? FinanceLegalGroupId : null;
            var financeExecutionType = isContract ? StepExecutionType.Parallel : StepExecutionType.Sequential;

            steps.Add(new ApprovalStepDefinition(
                index++,
                "FinanceApproval",
                FlowGateRoles.Finance,
                financeExecutionType,
                financeGroupId));

            if (isContract)
            {
                steps.Add(new ApprovalStepDefinition(
                    index++,
                    "LegalReview",
                    FlowGateRoles.Legal,
                    StepExecutionType.Parallel,
                    FinanceLegalGroupId));
            }
        }
        else if (string.Equals(context.ProcessType, "contract", StringComparison.OrdinalIgnoreCase))
        {
            steps.Add(new ApprovalStepDefinition(
                index++,
                "FinanceApproval",
                FlowGateRoles.Finance,
                StepExecutionType.Parallel,
                FinanceLegalGroupId));

            steps.Add(new ApprovalStepDefinition(
                index++,
                "LegalReview",
                FlowGateRoles.Legal,
                StepExecutionType.Parallel,
                FinanceLegalGroupId));
        }

        if (context.Amount > 10_000m)
        {
            steps.Add(new ApprovalStepDefinition(
                index++,
                "DirectorApproval",
                FlowGateRoles.Approver,
                StepExecutionType.Sequential,
                null));
        }

        return steps.AsReadOnly();
    }
}
