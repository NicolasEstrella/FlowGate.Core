using FlowGate.Core.Application.Engine;
using FlowGate.Core.Domain.Enums;
using FlowGate.Core.Infrastructure.Engine;
using FluentAssertions;

namespace FlowGate.Core.Tests.Unit.Engine;

public class RuleEngineTests
{
    private readonly IRuleEngine _sut;

    public RuleEngineTests()
    {
        _sut = new RuleEngineService();
    }

    [Fact]
    public void Evaluate_WhenAmountUnder5000_ShouldReturnOnlyManagerApproval()
    {
        // Arrange
        var context = new RuleContext("reimbursement", 1000m, "BRL", new Dictionary<string, string>());

        // Act
        var steps = _sut.Evaluate(context);

        // Assert
        steps.Should().HaveCount(1);
        steps[0].Name.Should().Be("ManagerApproval");
        steps[0].ExecutionType.Should().Be(StepExecutionType.Sequential);
        steps[0].StepIndex.Should().Be(0);
    }

    [Fact]
    public void Evaluate_WhenAmountOver5000_ShouldReturnManagerAndFinance()
    {
        // Arrange
        var context = new RuleContext("reimbursement", 6000m, "BRL", new Dictionary<string, string>());

        // Act
        var steps = _sut.Evaluate(context);

        // Assert
        steps.Should().HaveCount(2);
        steps[0].Name.Should().Be("ManagerApproval");
        steps[1].Name.Should().Be("FinanceApproval");
        steps[1].ExecutionType.Should().Be(StepExecutionType.Sequential);
        steps[1].StepIndex.Should().Be(1);
    }

    [Fact]
    public void Evaluate_WhenAmountOver10000_ShouldReturnManagerFinanceAndDirector()
    {
        // Arrange
        var context = new RuleContext("reimbursement", 15000m, "BRL", new Dictionary<string, string>());

        // Act
        var steps = _sut.Evaluate(context);

        // Assert
        steps.Should().HaveCount(3);
        steps[0].Name.Should().Be("ManagerApproval");
        steps[1].Name.Should().Be("FinanceApproval");
        steps[2].Name.Should().Be("DirectorApproval");
        steps[2].StepIndex.Should().Be(2);
    }

    [Fact]
    public void Evaluate_WhenContractType_ShouldAddLegalReviewParallelToFinance()
    {
        // Arrange
        var context = new RuleContext("contract", 8000m, "BRL", new Dictionary<string, string>());

        // Act
        var steps = _sut.Evaluate(context);

        // Assert
        steps.Should().HaveCount(3);
        steps[0].Name.Should().Be("ManagerApproval");
        steps[0].ExecutionType.Should().Be(StepExecutionType.Sequential);

        var financeStep = steps.FirstOrDefault(s => s.Name == "FinanceApproval");
        var legalStep = steps.FirstOrDefault(s => s.Name == "LegalReview");

        financeStep.Should().NotBeNull();
        legalStep.Should().NotBeNull();
        financeStep!.ExecutionType.Should().Be(StepExecutionType.Parallel);
        legalStep!.ExecutionType.Should().Be(StepExecutionType.Parallel);
        financeStep.GroupId.Should().Be(legalStep.GroupId);
        financeStep.GroupId.Should().NotBeNullOrEmpty();
    }
}
