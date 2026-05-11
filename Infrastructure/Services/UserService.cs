using FlowGate.Core.Application.Contracts;
using FlowGate.Core.Application.Services;
using FlowGate.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowGate.Core.Infrastructure.Services;

public sealed class UserService(FlowGateDbContext dbContext) : IUserService
{
    public async Task<IReadOnlyCollection<UserSummaryDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .OrderBy(user => user.DisplayName)
            .Select(user => new UserSummaryDto(
                user.Id,
                user.DisplayName,
                user.Email,
                user.IsActive,
                user.UserRoles
                    .Select(userRole => userRole.Role.Name)
                    .OrderBy(roleName => roleName)
                    .ToArray()))
            .ToArrayAsync(cancellationToken);
    }
}