using HordeR.Server;

namespace demo.Packets.ClientBound;
public struct PlayerConnectedPacket : IClientBoundPacket
{
    public string Type => nameof(PlayerConnectedPacket);
    public string Id { get; }
    public string Color { get; }

    public PlayerConnectedPacket(string id, string color)
    {
        Id = id;
        Color = color;
    }
}
