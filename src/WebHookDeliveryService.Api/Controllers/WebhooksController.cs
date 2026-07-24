using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebHookDeliveryService.Application.DTOs;
using WebHookDeliveryService.Application.Services;

namespace WebHookDeliveryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController(
    WebhookService webhookService,
    IValidator<CreateSubscriptionRequest> createValidator,
    IValidator<UpdateSubscriptionRequest> updateValidator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var result = await webhookService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await webhookService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await webhookService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubscriptionRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var result = await webhookService.UpdateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await webhookService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/regenerate-secret")]
    public async Task<IActionResult> RegenerateSecret(Guid id)
    {
        var result = await webhookService.RegenerateSecretAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
