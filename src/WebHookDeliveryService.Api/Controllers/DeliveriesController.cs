using Microsoft.AspNetCore.Mvc;
using WebHookDeliveryService.Application.Services;
using WebHookDeliveryService.Domain.Enums;

namespace WebHookDeliveryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveriesController(DeliveryService deliveryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DeliveryStatus? status = null,
        [FromQuery] Guid? subscriptionId = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var result = await deliveryService.GetAllAsync(status, subscriptionId, skip, take);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await deliveryService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id)
    {
        var success = await deliveryService.DispatchAsync(id);
        return Ok(new { success, message = success ? "Delivery succeeded" : "Delivery failed, will retry" });
    }
}
