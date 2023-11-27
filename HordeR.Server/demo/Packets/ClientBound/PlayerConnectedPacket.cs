using HordeR.Server.Packets;

namespace demo.Packets.ClientBound;
public struct PlayerConnectedPacket : IClientBoundPacket
{
    public string Type => nameof(PlayerConnectedPacket);
    public string Id { get; }
    public string Color { get; }
    public string Name { get; }
    public float X { get; }
    public float Y { get; }

    public PlayerConnectedPacket(Player player)
    {
        Id = player.Id;
        Color = player.Color;
        Name = player.Name;
        X = player.X;
        Y = player.Y;
    }
}
