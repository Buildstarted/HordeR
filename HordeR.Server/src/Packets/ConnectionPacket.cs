namespace HordeR.Server.Packets;

public class ConnectionPacket : IServerBoundPacket
{
    public static Func<PacketConstructorInfo, IServerBoundPacket> CreatePacket => (packet) => new ConnectionPacket(packet);
    public static int Type => (int)PacketType.Connect;

    public Connection Connection { get; }

    public ConnectionPacket(PacketConstructorInfo packet)
    {
        Connection = packet.Connection;
    }
}
