using Microsoft.AspNetCore.SignalR;

namespace HubService;

public class ChatHub : Hub<OnChatMessage>
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }


    
    public async Task SendMessage(string user, string message)
        => await Clients.All.ReceiveChatMessage(user, message);
}