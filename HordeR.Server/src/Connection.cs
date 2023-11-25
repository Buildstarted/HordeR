using Microsoft.AspNetCore.SignalR;

namespace HordeR.Server;

public class Connection
{
    private readonly IHubContext<GameHub> hub;
    private short ping;
    private string connectionId;

    public string ConnectionId => connectionId;

    public Connection(string connectionId, IHubContext<GameHub> hub)
    {
        this.connectionId = connectionId;
        this.hub = hub;
    }

    public void SendAsync(string method, params object[] args)
    {
        hub.Clients.Client(connectionId).SendAsync(method, args);
    }

    public void SetPing(long pingDiff)
    {
        ping = (short)pingDiff;
    }
}
