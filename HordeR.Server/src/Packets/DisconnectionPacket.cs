namespace HordeR.Server.Packets;

using PacketBuilder = Func<PacketConstructorInfo, IServerBoundPacket>;

public class DisconnectionPacket : IServerBoundPacket
{
    public static PacketBuilder CreatePacket => (packet) => new DisconnectionPacket(packet);
    public static int Type => (int)PacketType.Disconnect;

    public Connection Connection { get; }

    public DisconnectionPacket(PacketConstructorInfo packet)
    {
        Connection = packet.Connection;
    }
}
