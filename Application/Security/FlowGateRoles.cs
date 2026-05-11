namespace FlowGate.Core.Application.Security;

public static class FlowGateRoles
{
    public const string Admin = nameof(Admin);
    public const string Approver = nameof(Approver);
    public const string Finance = nameof(Finance);
    public const string Legal = nameof(Legal);
    public const string StandardUser = nameof(StandardUser);

    public static readonly string[] All =
    [
        Admin,
        Approver,
        Finance,
        Legal,
        StandardUser
    ];
}