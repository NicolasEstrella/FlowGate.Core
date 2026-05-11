using FlowGate.Core.Application.Contracts;

namespace FlowGate.Core.Application.Services;

public interface IUserService
{
    Task<IReadOnlyCollection<UserSummaryDto>> GetUsersAsync(CancellationToken cancellationToken);
}