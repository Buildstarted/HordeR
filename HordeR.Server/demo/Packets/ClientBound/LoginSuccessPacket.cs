using HordeR.Server.Packets;

namespace demo.Packets.ClientBound;

public struct LoginSuccessPacket : IClientBoundPacket
{
    public string Type => nameof(LoginSuccessPacket);
    public string Id { get; }
    public string Color { get; }
    public string Name { get; }
    public float X { get; }
    public float Y { get; }

    public LoginSuccessPacket(Player player)
    {
        Id = player.Id;
        Color = player.Color;
        Name = player.Name;
        X = player.X;
        Y = player.Y;
    }
}
