using WebHookDeliveryService.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<DeliveryWorker>();

var host = builder.Build();
host.Run();
