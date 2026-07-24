using Microsoft.AspNetCore.Mvc;
using WebHookDeliveryService.Application.Services;

namespace WebHookDeliveryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeadLettersController(DeadLetterService deadLetterService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var result = await deadLetterService.GetAllAsync(skip, take);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await deadLetterService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/replay")]
    public async Task<IActionResult> Replay(Guid id)
    {
        var success = await deadLetterService.ReplayAsync(id);
        return success ? Ok(new { message = "Replay queued" }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Dismiss(Guid id)
    {
        var success = await deadLetterService.DismissAsync(id);
        return success ? NoContent() : NotFound();
    }
}
