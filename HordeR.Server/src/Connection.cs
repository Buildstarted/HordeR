using Microsoft.AspNetCore.SignalR;

namespace HordeR.Server;

public class Connection
{
    private short ping;
    private string connectionId;
    private readonly GameServer server;

    public string ConnectionId => connectionId;

    public Connection(string connectionId, GameServer server)
    {
        this.connectionId = connectionId;
        this.server = server;
    }

    public void Send<T>(IEnumerable<T> packets) where T : IClientBoundPacket
    {
        server.Send(this, packets);
    }

    public void Send<T>(params T[] packets) where T : IClientBoundPacket
    {
        server.Send(this, packets);
    }

    public void SetPing(long pingDiff)
    {
        ping = (short)pingDiff;
    }
}
