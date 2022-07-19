namespace HubService
{
    public interface OnChatMessage
    {
        Task ReceiveChatMessage(string user, string message);
    }
}
