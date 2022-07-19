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

Console.ReadLine();