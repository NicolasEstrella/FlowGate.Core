using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowGate.Core.Infrastructure.Security;

public sealed class DevelopmentHeaderAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "DevelopmentHeaders";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers["X-FlowGate-UserId"].FirstOrDefault()
            ?? configuration["Authentication:DefaultUserId"];

        var userName = Request.Headers["X-FlowGate-UserName"].FirstOrDefault()
            ?? configuration["Authentication:DefaultUserName"]
            ?? "Local User";

        var userEmail = Request.Headers["X-FlowGate-UserEmail"].FirstOrDefault()
            ?? configuration["Authentication:DefaultUserEmail"]
            ?? "user@flowgate.local";

        var configuredRoles = configuration.GetSection("Authentication:DefaultRoles").Get<string[]>() ?? [];
        var headerRoles = Request.Headers["X-FlowGate-Roles"].FirstOrDefault();
        var roles = string.IsNullOrWhiteSpace(headerRoles)
            ? configuredRoles
            : headerRoles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (string.IsNullOrWhiteSpace(userId) || roles.Length == 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, userEmail)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}