using Microsoft.AspNetCore.SignalR;

namespace WebHookDeliveryService.Dashboard.Hubs;

public class DashboardHub : Hub
{
    public async Task SendDeliveryUpdate(object update)
    {
        await Clients.All.SendAsync("ReceiveDeliveryUpdate", update);
    }

    public async Task SendStatsUpdate(object stats)
    {
        await Clients.All.SendAsync("ReceiveStatsUpdate", stats);
    }
}
