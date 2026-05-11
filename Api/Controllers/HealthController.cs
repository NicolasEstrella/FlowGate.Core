using FlowGate.Core.Api.Middleware;
using FlowGate.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowGate.Core.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/health")]
public sealed class HealthController(IHealthService healthService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.TraceIdentifier;
        var health = await healthService.GetStatusAsync(correlationId, cancellationToken);
        return Ok(health);
    }
}