namespace FlowGate.Core.Application.Security;

public static class AuthorizationPolicies
{
    public const string Admin = nameof(Admin);
    public const string Approver = nameof(Approver);
    public const string Finance = nameof(Finance);
    public const string Legal = nameof(Legal);
    public const string StandardUser = nameof(StandardUser);
    public const string WorkflowReaders = nameof(WorkflowReaders);
}