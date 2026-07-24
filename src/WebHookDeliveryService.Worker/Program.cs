using WebHookDeliveryService.Application.Extensions;
using WebHookDeliveryService.Infrastructure.Extensions;
using WebHookDeliveryService.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWebhookApplication();
builder.Services.AddWebhookInfrastructure(builder.Configuration);

builder.Services.AddHostedService<DeliveryWorker>();
builder.Services.AddHostedService<RetryWorker>();

var host = builder.Build();
host.Run();
