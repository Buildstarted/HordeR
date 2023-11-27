namespace HordeR.Server.Packets;

public class DisconnectionPacket : IServerBoundPacket
{
    public static Func<PacketConstructorInfo, IServerBoundPacket> CreatePacket => (packet) => new DisconnectionPacket(packet);
    public static int Type => (int)PacketType.Disconnect;

    public Connection Connection { get; }

    public DisconnectionPacket(PacketConstructorInfo packet)
    {
        Connection = packet.Connection;
    }
}