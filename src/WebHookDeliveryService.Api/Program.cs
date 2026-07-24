using Microsoft.OpenApi.Models;
using WebHookDeliveryService.Application.Extensions;
using WebHookDeliveryService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Webhook Delivery Service",
        Version = "v1",
        Description = "A reliable webhook delivery system with retry queues and HMAC verification"
    });
});

builder.Services.AddWebhookApplication();
builder.Services.AddWebhookInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
