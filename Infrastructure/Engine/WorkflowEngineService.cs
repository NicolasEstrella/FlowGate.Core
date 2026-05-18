using System.Text.Json;
using FlowGate.Core.Application.Engine;
using FlowGate.Core.Domain.Entities;
using FlowGate.Core.Domain.Enums;

namespace FlowGate.Core.Infrastructure.Engine;

public sealed class WorkflowEngineService(
    IWorkflowStore store,
    IRuleEngine ruleEngine,
    IWorkflowStateMachine stateMachine) : IWorkflowEngine
{
    public async Task<Guid> StartAsync(CreateRequestCommand command, CancellationToken cancellationToken = default)
    {
        var workflow = await store.FindWorkflowByKeyAsync(command.WorkflowKey, cancellationToken)
            ?? throw new InvalidOperationException($"Workflow '{command.WorkflowKey}' not found or inactive.");

        var formDataJson = JsonSerializer.Serialize(command.FormData);

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflow.Id,
            RequesterId = command.RequesterId,
            Title = command.Title,
            CurrentStatus = WorkflowInstanceStatus.Draft,
            FormDataJson = formDataJson,
            CreatedBy = command.RequesterId.ToString()
        };

        store.AddInstance(instance);
        await store.SaveChangesAsync(cancellationToken);

        return instance.Id;
    }

    public async Task SubmitAsync(Guid instanceId, string performedBy, CancellationToken cancellationToken = default)
    {
        var instance = await store.FindInstanceAsync(instanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Workflow instance '{instanceId}' not found.");

        stateMachine.TransitionTo(instance, WorkflowInstanceStatus.Submitted, performedBy);

        var ruleContext = BuildRuleContext(instance);
        var stepDefinitions = ruleEngine.Evaluate(ruleContext);

        foreach (var definition in stepDefinitions)
        {
            var step = new ApprovalStep
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                StepIndex = definition.StepIndex,
                Order = definition.StepIndex,
                Name = definition.Name,
                RequiredRole = definition.RequiredRole,
                ExecutionType = definition.ExecutionType,
                GroupId = definition.GroupId,
                Status = ApprovalStepStatus.Pending,
                CreatedBy = "system"
            };
            store.AddStep(step);
            instance.ApprovalSteps.Add(step);
        }

        ActivateNextSteps(instance);

        stateMachine.TransitionTo(instance, WorkflowInstanceStatus.InApproval, "system");
        instance.SubmittedAtUtc = DateTimeOffset.UtcNow;

        store.AddAuditLog(BuildAuditLog(instance, WorkflowInstanceStatus.Draft, WorkflowInstanceStatus.InApproval, performedBy));

        await store.SaveChangesAsync(cancellationToken);
    }

    public async Task ProcessDecisionAsync(SubmitDecisionCommand command, CancellationToken cancellationToken = default)
    {
        var instance = await store.FindInstanceAsync(command.WorkflowInstanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Workflow instance '{command.WorkflowInstanceId}' not found.");

        var step = instance.ApprovalSteps.FirstOrDefault(s => s.Id == command.StepId)
            ?? throw new InvalidOperationException($"Step '{command.StepId}' not found on instance.");

        var previousStatus = instance.CurrentStatus;

        ApplyDecisionToStep(step, command);

        if (command.Decision == DecisionType.Reject)
        {
            stateMachine.TransitionTo(instance, WorkflowInstanceStatus.Rejected, command.PerformedBy, command.Comment);
            instance.CompletedAtUtc = DateTimeOffset.UtcNow;
        }
        else if (command.Decision == DecisionType.RequestAdjustment)
        {
            stateMachine.TransitionTo(instance, WorkflowInstanceStatus.AdjustmentsRequested, command.PerformedBy, command.Comment);
        }
        else
        {
            TryAdvanceWorkflow(instance, step, command.PerformedBy);
        }

        store.AddAuditLog(BuildAuditLog(instance, previousStatus, instance.CurrentStatus, command.PerformedBy, command.Comment));
        await store.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid instanceId, string performedBy, CancellationToken cancellationToken = default)
    {
        var instance = await store.FindInstanceAsync(instanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Workflow instance '{instanceId}' not found.");

        var previousStatus = instance.CurrentStatus;
        stateMachine.TransitionTo(instance, WorkflowInstanceStatus.Cancelled, performedBy);
        instance.CompletedAtUtc = DateTimeOffset.UtcNow;

        store.AddAuditLog(BuildAuditLog(instance, previousStatus, WorkflowInstanceStatus.Cancelled, performedBy));
        await store.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyDecisionToStep(ApprovalStep step, SubmitDecisionCommand command)
    {
        step.Status = command.Decision switch
        {
            DecisionType.Approve => ApprovalStepStatus.Approved,
            DecisionType.Reject => ApprovalStepStatus.Rejected,
            DecisionType.RequestAdjustment => ApprovalStepStatus.AdjustmentsRequested,
            _ => throw new ArgumentOutOfRangeException(nameof(command.Decision))
        };
        step.DecidedAtUtc = DateTimeOffset.UtcNow;
        step.DecisionComment = command.Comment;
        step.ModifiedBy = command.PerformedBy;
        step.ModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    private void TryAdvanceWorkflow(WorkflowInstance instance, ApprovalStep decidedStep, string performedBy)
    {
        if (decidedStep.ExecutionType == StepExecutionType.Parallel && !string.IsNullOrEmpty(decidedStep.GroupId))
        {
            var groupSteps = instance.ApprovalSteps
                .Where(s => s.GroupId == decidedStep.GroupId)
                .ToList();

            bool allGroupDone = groupSteps.All(s => s.Status == ApprovalStepStatus.Approved);
            if (!allGroupDone)
                return;
        }

        var nextStep = instance.ApprovalSteps
            .Where(s => s.Status == ApprovalStepStatus.Pending)
            .OrderBy(s => s.StepIndex)
            .FirstOrDefault();

        if (nextStep is not null)
        {
            nextStep.Status = ApprovalStepStatus.InProgress;
            return;
        }

        bool allApproved = instance.ApprovalSteps.All(s => s.Status == ApprovalStepStatus.Approved);
        if (allApproved)
        {
            stateMachine.TransitionTo(instance, WorkflowInstanceStatus.Approved, performedBy);
            instance.CompletedAtUtc = DateTimeOffset.UtcNow;
        }
    }

    private static void ActivateNextSteps(WorkflowInstance instance)
    {
        var orderedSteps = instance.ApprovalSteps.OrderBy(s => s.StepIndex).ToList();
        if (!orderedSteps.Any())
            return;

        var firstStep = orderedSteps.First();
        if (firstStep.ExecutionType == StepExecutionType.Parallel && !string.IsNullOrEmpty(firstStep.GroupId))
        {
            var groupId = firstStep.GroupId;
            foreach (var groupStep in orderedSteps.Where(s => s.GroupId == groupId))
                groupStep.Status = ApprovalStepStatus.InProgress;
        }
        else
        {
            firstStep.Status = ApprovalStepStatus.InProgress;
        }
    }

    private static RuleContext BuildRuleContext(WorkflowInstance instance)
    {
        var formData = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(instance.FormDataJson))
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(instance.FormDataJson);
            if (parsed is not null)
                foreach (var kv in parsed)
                    formData[kv.Key] = kv.Value;
        }

        decimal.TryParse(formData.GetValueOrDefault("amount", "0"), out var amount);
        var processType = formData.GetValueOrDefault("processType", "reimbursement");

        return new RuleContext(processType, amount, "BRL", formData);
    }

    private static AuditLog BuildAuditLog(
        WorkflowInstance instance,
        WorkflowInstanceStatus previousStatus,
        WorkflowInstanceStatus newStatus,
        string performedBy,
        string? comment = null)
    {
        var details = JsonSerializer.Serialize(new
        {
            previousState = previousStatus.ToString(),
            nextState = newStatus.ToString(),
            comment
        });

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = nameof(WorkflowInstance),
            EntityId = instance.Id,
            Action = $"Transition:{previousStatus}->{newStatus}",
            WorkflowInstanceId = instance.Id,
            PerformedByUserId = null,
            DetailsJson = details,
            CreatedBy = performedBy
        };
    }
}
