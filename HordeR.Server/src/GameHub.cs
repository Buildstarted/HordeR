using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Nodes;

namespace HordeR.Server;

public class GameHub : Hub
{
    private readonly GameServer server;

    public GameHub(GameServer server)
    {
        this.server = server;
    }

    public override Task OnConnectedAsync()
    {
        var client = server.CreateConnection(Context.ConnectionId);
        server.OnPacketReceived(client, (int)PacketType.Connect);

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var client = server.RemoveConnection(Context.ConnectionId);
        server.OnPacketReceived(client, (int)PacketType.Disconnect);
        return base.OnDisconnectedAsync(exception);
    }


    public object GetSettings()
    {
        return new
        {
            tps = server.TPS,
            worldTick = server.WorldTick
        };
    }

    public object Sync(long clientTick)
    {
        var connection = server.GetConnection(Context.ConnectionId);
        connection.SetPing(server.WorldTick - clientTick);

        return new { serverTick = server.WorldTick, offset = server.WorldTick - clientTick };
    }

    public void Packet(JsonNode json)
    {
        //get the client
        var client = server.GetConnection(Context.ConnectionId);
        if (client is not null)
        {
            var packetType = json["type"].GetValue<int>();
            server.OnPacketReceived(client, packetType, json);
        }
    }
}
