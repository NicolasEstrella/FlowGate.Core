using FlowGate.Core.Application.Engine;
using FlowGate.Core.Domain.Entities;
using FlowGate.Core.Domain.Enums;
using FlowGate.Core.Infrastructure.Engine;
using FluentAssertions;

namespace FlowGate.Core.Tests.Unit.Engine;

public class WorkflowStateMachineTests
{
    private readonly IWorkflowStateMachine _sut;

    public WorkflowStateMachineTests()
    {
        _sut = new WorkflowStateMachineService();
    }

    private static WorkflowInstance CreateInstance(WorkflowInstanceStatus status) =>
        new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            WorkflowId = Guid.NewGuid(),
            RequesterId = Guid.NewGuid(),
            Title = "Test",
            CurrentStatus = status,
            CreatedBy = "system",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

    [Fact]
    public void TransitionTo_FromDraftToSubmitted_ShouldUpdateStatus()
    {
        // Arrange
        var instance = CreateInstance(WorkflowInstanceStatus.Draft);

        // Act
        _sut.TransitionTo(instance, WorkflowInstanceStatus.Submitted, "user1");

        // Assert
        instance.CurrentStatus.Should().Be(WorkflowInstanceStatus.Submitted);
    }

    [Fact]
    public void TransitionTo_FromSubmittedToInApproval_ShouldUpdateStatus()
    {
        // Arrange
        var instance = CreateInstance(WorkflowInstanceStatus.Submitted);

        // Act
        _sut.TransitionTo(instance, WorkflowInstanceStatus.InApproval, "system");

        // Assert
        instance.CurrentStatus.Should().Be(WorkflowInstanceStatus.InApproval);
    }

    [Fact]
    public void TransitionTo_FromInApprovalToApproved_ShouldUpdateStatus()
    {
        // Arrange
        var instance = CreateInstance(WorkflowInstanceStatus.InApproval);

        // Act
        _sut.TransitionTo(instance, WorkflowInstanceStatus.Approved, "approver1");

        // Assert
        instance.CurrentStatus.Should().Be(WorkflowInstanceStatus.Approved);
    }

    [Fact]
    public void TransitionTo_FromInApprovalToAdjustmentsRequested_ShouldUpdateStatus()
    {
        // Arrange
        var instance = CreateInstance(WorkflowInstanceStatus.InApproval);

        // Act
        _sut.TransitionTo(instance, WorkflowInstanceStatus.AdjustmentsRequested, "approver1");

        // Assert
        instance.CurrentStatus.Should().Be(WorkflowInstanceStatus.AdjustmentsRequested);
    }

    [Fact]
    public void TransitionTo_FromAdjustmentsRequestedToSubmitted_ShouldUpdateStatus()
    {
        // Arrange
        var instance = CreateInstance(WorkflowInstanceStatus.AdjustmentsRequested);

        // Act
        _sut.TransitionTo(instance, WorkflowInstanceStatus.Submitted, "requester1");

        // Assert
        instance.CurrentStatus.Should().Be(WorkflowInstanceStatus.Submitted);
    }

    [Fact]
    public void TransitionTo_IllegalTransition_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var instance = CreateInstance(WorkflowInstanceStatus.Draft);

        // Act
        var act = () => _sut.TransitionTo(instance, WorkflowInstanceStatus.Approved, "user1");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
