using Microsoft.AspNetCore.Mvc;
using WebHookDeliveryService.Application.Services;

namespace WebHookDeliveryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(DashboardService dashboardService) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await dashboardService.GetStatsAsync();
        return Ok(result);
    }
}
