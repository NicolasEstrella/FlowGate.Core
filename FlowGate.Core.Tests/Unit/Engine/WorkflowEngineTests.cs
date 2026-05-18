using FlowGate.Core.Application.Engine;
using FlowGate.Core.Domain.Entities;
using FlowGate.Core.Domain.Enums;
using FluentAssertions;
using Moq;

namespace FlowGate.Core.Tests.Unit.Engine;

public class WorkflowEngineTests
{
    private readonly Mock<IRuleEngine> _ruleEngineMock;
    private readonly Mock<IWorkflowStateMachine> _stateMachineMock;
    private readonly FakeWorkflowEngineContext _context;
    private readonly IWorkflowEngine _sut;

    public WorkflowEngineTests()
    {
        _ruleEngineMock = new Mock<IRuleEngine>();
        _stateMachineMock = new Mock<IWorkflowStateMachine>();
        _context = new FakeWorkflowEngineContext();
        _sut = new FlowGate.Core.Infrastructure.Engine.WorkflowEngineService(
            _context,
            _ruleEngineMock.Object,
            _stateMachineMock.Object);
    }

    [Fact]
    public async Task StartAsync_WithValidCommand_ShouldCreateDraftInstanceAndReturnId()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        _context.AddWorkflow(new Workflow { Id = workflowId, Key = "reimbursement", Name = "Reimbursement", Version = 1, IsActive = true, CreatedBy = "system" });
        _context.AddUser(new User { Id = requesterId, Email = "user@test.com", DisplayName = "Test User", CreatedBy = "system" });

        var command = new CreateRequestCommand("reimbursement", "My Request", new Dictionary<string, string> { { "amount", "1000" } }, requesterId);

        // Act
        var instanceId = await _sut.StartAsync(command);

        // Assert
        instanceId.Should().NotBeEmpty();
        _context.WorkflowInstances.Should().HaveCount(1);
        _context.WorkflowInstances[0].CurrentStatus.Should().Be(WorkflowInstanceStatus.Draft);
        _context.WorkflowInstances[0].Title.Should().Be("My Request");
    }

    [Fact]
    public async Task SubmitAsync_WithDraftInstance_ShouldCreateStepsAndTransitionToInApproval()
    {
        // Arrange
        var instance = CreateDraftInstance();
        _context.WorkflowInstances.Add(instance);
        _context.AddWorkflow(new Workflow { Id = instance.WorkflowId, Key = "reimbursement", Name = "Reimbursement", Version = 1, IsActive = true, CreatedBy = "system" });

        _ruleEngineMock
            .Setup(r => r.Evaluate(It.IsAny<RuleContext>()))
            .Returns([new ApprovalStepDefinition(0, "ManagerApproval", "Approver", StepExecutionType.Sequential, null)]);

        _stateMachineMock
            .Setup(s => s.TransitionTo(instance, WorkflowInstanceStatus.Submitted, It.IsAny<string>(), null))
            .Callback<WorkflowInstance, WorkflowInstanceStatus, string, string?>((i, s, _, _) => i.CurrentStatus = s);
        _stateMachineMock
            .Setup(s => s.TransitionTo(instance, WorkflowInstanceStatus.InApproval, It.IsAny<string>(), null))
            .Callback<WorkflowInstance, WorkflowInstanceStatus, string, string?>((i, s, _, _) => i.CurrentStatus = s);

        // Act
        await _sut.SubmitAsync(instance.Id, "requester1");

        // Assert
        _stateMachineMock.Verify(s => s.TransitionTo(instance, WorkflowInstanceStatus.Submitted, It.IsAny<string>(), null), Times.Once);
        _stateMachineMock.Verify(s => s.TransitionTo(instance, WorkflowInstanceStatus.InApproval, It.IsAny<string>(), null), Times.Once);
        _context.ApprovalSteps.Should().HaveCount(1);
        _context.AuditLogs.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProcessDecisionAsync_ApproveLastStep_ShouldTransitionToApproved()
    {
        // Arrange
        var instance = CreateInstanceWithStatus(WorkflowInstanceStatus.InApproval);
        var step = CreateStep(instance.Id, 0, ApprovalStepStatus.InProgress);
        instance.ApprovalSteps.Add(step);
        _context.WorkflowInstances.Add(instance);
        _context.ApprovalSteps.Add(step);

        _stateMachineMock
            .Setup(s => s.TransitionTo(instance, WorkflowInstanceStatus.Approved, It.IsAny<string>(), null))
            .Callback<WorkflowInstance, WorkflowInstanceStatus, string, string?>((i, s, _, _) => i.CurrentStatus = s);

        var command = new SubmitDecisionCommand(instance.Id, step.Id, DecisionType.Approve, "approver1", null);

        // Act
        await _sut.ProcessDecisionAsync(command);

        // Assert
        step.Status.Should().Be(ApprovalStepStatus.Approved);
        _stateMachineMock.Verify(s => s.TransitionTo(instance, WorkflowInstanceStatus.Approved, It.IsAny<string>(), null), Times.Once);
    }

    [Fact]
    public async Task ProcessDecisionAsync_RejectStep_ShouldTransitionToRejected()
    {
        // Arrange
        var instance = CreateInstanceWithStatus(WorkflowInstanceStatus.InApproval);
        var step = CreateStep(instance.Id, 0, ApprovalStepStatus.InProgress);
        instance.ApprovalSteps.Add(step);
        _context.WorkflowInstances.Add(instance);
        _context.ApprovalSteps.Add(step);

        _stateMachineMock
            .Setup(s => s.TransitionTo(instance, WorkflowInstanceStatus.Rejected, It.IsAny<string>(), It.IsAny<string?>()))
            .Callback<WorkflowInstance, WorkflowInstanceStatus, string, string?>((i, s, _, _) => i.CurrentStatus = s);

        var command = new SubmitDecisionCommand(instance.Id, step.Id, DecisionType.Reject, "approver1", "Not valid");

        // Act
        await _sut.ProcessDecisionAsync(command);

        // Assert
        step.Status.Should().Be(ApprovalStepStatus.Rejected);
        _stateMachineMock.Verify(s => s.TransitionTo(instance, WorkflowInstanceStatus.Rejected, It.IsAny<string>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDecisionAsync_ApproveFirstParallelStep_ShouldNotAdvanceInstance()
    {
        // Arrange
        var instance = CreateInstanceWithStatus(WorkflowInstanceStatus.InApproval);
        var step1 = CreateStep(instance.Id, 0, ApprovalStepStatus.InProgress, StepExecutionType.Parallel, "group-a");
        var step2 = CreateStep(instance.Id, 1, ApprovalStepStatus.InProgress, StepExecutionType.Parallel, "group-a");
        instance.ApprovalSteps.Add(step1);
        instance.ApprovalSteps.Add(step2);
        _context.WorkflowInstances.Add(instance);
        _context.ApprovalSteps.Add(step1);
        _context.ApprovalSteps.Add(step2);

        var command = new SubmitDecisionCommand(instance.Id, step1.Id, DecisionType.Approve, "approver1", null);

        // Act
        await _sut.ProcessDecisionAsync(command);

        // Assert
        step1.Status.Should().Be(ApprovalStepStatus.Approved);
        instance.CurrentStatus.Should().Be(WorkflowInstanceStatus.InApproval);
        _stateMachineMock.Verify(s => s.TransitionTo(It.IsAny<WorkflowInstance>(), It.IsAny<WorkflowInstanceStatus>(), It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task ProcessDecisionAsync_ApproveAllParallelSteps_ShouldAdvanceToApproved()
    {
        // Arrange
        var instance = CreateInstanceWithStatus(WorkflowInstanceStatus.InApproval);
        var step1 = CreateStep(instance.Id, 0, ApprovalStepStatus.Approved, StepExecutionType.Parallel, "group-a");
        var step2 = CreateStep(instance.Id, 1, ApprovalStepStatus.InProgress, StepExecutionType.Parallel, "group-a");
        instance.ApprovalSteps.Add(step1);
        instance.ApprovalSteps.Add(step2);
        _context.WorkflowInstances.Add(instance);
        _context.ApprovalSteps.Add(step1);
        _context.ApprovalSteps.Add(step2);

        _stateMachineMock
            .Setup(s => s.TransitionTo(instance, WorkflowInstanceStatus.Approved, It.IsAny<string>(), null))
            .Callback<WorkflowInstance, WorkflowInstanceStatus, string, string?>((i, s, _, _) => i.CurrentStatus = s);

        var command = new SubmitDecisionCommand(instance.Id, step2.Id, DecisionType.Approve, "approver2", null);

        // Act
        await _sut.ProcessDecisionAsync(command);

        // Assert
        step2.Status.Should().Be(ApprovalStepStatus.Approved);
        _stateMachineMock.Verify(s => s.TransitionTo(instance, WorkflowInstanceStatus.Approved, It.IsAny<string>(), null), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_WithDraftInstance_ShouldTransitionToCancelled()
    {
        // Arrange
        var instance = CreateDraftInstance();
        _context.WorkflowInstances.Add(instance);

        _stateMachineMock
            .Setup(s => s.TransitionTo(instance, WorkflowInstanceStatus.Cancelled, It.IsAny<string>(), null))
            .Callback<WorkflowInstance, WorkflowInstanceStatus, string, string?>((i, s, _, _) => i.CurrentStatus = s);

        // Act
        await _sut.CancelAsync(instance.Id, "user1");

        // Assert
        _stateMachineMock.Verify(s => s.TransitionTo(instance, WorkflowInstanceStatus.Cancelled, It.IsAny<string>(), null), Times.Once);
        _context.AuditLogs.Should().NotBeEmpty();
    }

    private WorkflowInstance CreateDraftInstance() =>
        new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            WorkflowId = Guid.NewGuid(),
            RequesterId = Guid.NewGuid(),
            Title = "Test Instance",
            CurrentStatus = WorkflowInstanceStatus.Draft,
            FormDataJson = "{\"amount\":\"1000\",\"processType\":\"reimbursement\"}",
            CreatedBy = "user1"
        };

    private WorkflowInstance CreateInstanceWithStatus(WorkflowInstanceStatus status)
    {
        var instance = CreateDraftInstance();
        instance.CurrentStatus = status;
        return instance;
    }

    private ApprovalStep CreateStep(
        Guid instanceId,
        int index,
        ApprovalStepStatus status,
        StepExecutionType executionType = StepExecutionType.Sequential,
        string? groupId = null) =>
        new ApprovalStep
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instanceId,
            StepIndex = index,
            Order = index,
            Name = $"Step{index}",
            RequiredRole = "Approver",
            Status = status,
            ExecutionType = executionType,
            GroupId = groupId,
            CreatedBy = "system"
        };
}
