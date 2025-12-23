using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

class Program
{
    // The ASCII Record Separator character (30) required by SignalR
    private const char RecordSeparator = (char)0x1e;

    static async Task Main(string[] args)
    {
        // 1. Connect to the SignalR Endpoint via WS
        var uri = new Uri("ws://localhost:5196/chat");
        using var client = new ClientWebSocket();

        Console.WriteLine("Connecting to Server...");
        await client.ConnectAsync(uri, CancellationToken.None);
        Console.WriteLine("Connected.");

        // 2. The Handshake
        // You MUST send this specific JSON + Separator immediately to negotiate the protocol.
        var handshake = JsonSerializer.Serialize(new { protocol = "json", version = 1 }) + RecordSeparator;
        await SendRawMessage(client, handshake);

        // Start a background task to listen for incoming messages
        _ = Task.Run(() => ReceiveLoop(client));

        Console.WriteLine("Type a message and press Enter to send (or 'exit' to quit):");

        while (true)
        {
            var message = Console.ReadLine();
            if (message == "exit") break;

            // 3. Send Invocation Message
            // To call a method on the hub, we send a JSON object with:
            // type: 1 (Invocation)
            // target: The method name on the Hub ("SendMessage")
            // arguments: Array of arguments for that method

            var payload = new
            {
                type = 1,
                target = "SendMessage",
                arguments = new[] { "RawClient", message }
            };

            var jsonPayload = JsonSerializer.Serialize(payload) + RecordSeparator;
            await SendRawMessage(client, jsonPayload);
        }

        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Byee", CancellationToken.None);
    }

    private static async Task SendRawMessage(ClientWebSocket client, string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        await client.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    private static async Task ReceiveLoop(ClientWebSocket client)
    {
        var buffer = new byte[1024 * 4];

        while (client.State == WebSocketState.Open)
        {
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close) break;

            var response = Encoding.UTF8.GetString(buffer, 0, result.Count);

            // SignalR might send multiple messages in one packet, split by the separator
            var messages = response.Split(RecordSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var msg in messages)
            {
                // Simple parsing to see what the server sent back
                // In a real app, you would deserialize this JSON
                if (msg == "{}")
                {
                    Console.WriteLine("[Client] Handshake confirmed by server.");
                }
                else if (msg.Contains("\"type\":6"))
                {
                    // Type 6 is a Keep-Alive Ping from server (ignore it or log it)
                }
                else
                {
                    Console.WriteLine($"[Client] Received: {msg}");
                }
            }
        }
    }
}