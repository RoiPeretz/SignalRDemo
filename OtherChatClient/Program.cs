using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7196/Chat")
    .WithAutomaticReconnect()
    .Build();

connection.StartAsync();


connection.On<string, string>("ReceiveChatMessage", (user, message) =>
{
    Console.WriteLine($"{user}: {message}");
});

for (var i = 0; i < 100; i++)
{
    connection.SendAsync("SendMessage", "OtherChatClient", "Weeeeeeeeee");
}

Console.ReadLine();