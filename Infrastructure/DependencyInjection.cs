using System.Security.Claims;
using System.Text.Json.Serialization;
using FlowGate.Core.Application.Security;
using FlowGate.Core.Application.Services;
using FlowGate.Core.Infrastructure.Persistence;
using FlowGate.Core.Infrastructure.Security;
using FlowGate.Core.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowGate.Core.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<FlowGateDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IHealthService, HealthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWorkflowService, WorkflowService>();

        services
            .AddAuthentication(DevelopmentHeaderAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, DevelopmentHeaderAuthenticationHandler>(
                DevelopmentHeaderAuthenticationHandler.SchemeName,
                _ => { });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.Admin, policy => policy.RequireRole(FlowGateRoles.Admin));
            options.AddPolicy(AuthorizationPolicies.Approver, policy => policy.RequireRole(FlowGateRoles.Approver));
            options.AddPolicy(AuthorizationPolicies.Finance, policy => policy.RequireRole(FlowGateRoles.Finance));
            options.AddPolicy(AuthorizationPolicies.Legal, policy => policy.RequireRole(FlowGateRoles.Legal));
            options.AddPolicy(AuthorizationPolicies.StandardUser, policy => policy.RequireRole(FlowGateRoles.StandardUser));
            options.AddPolicy(AuthorizationPolicies.WorkflowReaders, policy =>
                policy.RequireAssertion(context =>
                    HasAnyRole(context.User,
                        FlowGateRoles.Admin,
                        FlowGateRoles.Approver,
                        FlowGateRoles.Finance,
                        FlowGateRoles.Legal,
                        FlowGateRoles.StandardUser)));
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }

    private static bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
    {
        return roles.Any(user.IsInRole);
    }
}