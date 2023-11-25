using HordeR.Server;

namespace demo.Packets.ClientBound;

public class PlayerDisconnectedPacket : IClientBoundPacket
{
    public string Type => nameof(PlayerDisconnectedPacket);

    public string Id { get; }

    public PlayerDisconnectedPacket(string id)
    {
        Id = id;
    }
}
