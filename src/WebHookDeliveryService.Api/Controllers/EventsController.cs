using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebHookDeliveryService.Application.DTOs;
using WebHookDeliveryService.Application.Services;

namespace WebHookDeliveryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(
    EventIngestionService eventIngestionService,
    IValidator<EventIngestRequest> eventValidator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Ingest([FromBody] EventIngestRequest request)
    {
        var validation = await eventValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var result = await eventIngestionService.IngestAsync(request);
        if (result is null)
            return Ok(new { message = "Event deduplicated (duplicate idempotency key)" });

        return Accepted(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var result = await eventIngestionService.GetAllAsync(skip, take);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await eventIngestionService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
