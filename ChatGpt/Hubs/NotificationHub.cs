using Microsoft.AspNetCore.SignalR;

namespace ChatGpt.Hubs;

public class NotificationHub : Hub
{
    public override Task OnConnectedAsync()
    {
        ;
        return base.OnConnectedAsync();
    }
}