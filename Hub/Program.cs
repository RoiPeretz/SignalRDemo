using HubService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();
app.MapHub<ChatHub>("/Chat");
app.Run();