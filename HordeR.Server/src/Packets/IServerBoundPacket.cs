namespace HordeR.Server.Packets;

public interface IServerBoundPacket
{
    Connection Connection { get; }
    static virtual int Type => throw new Exception("Packet type not set");
    static virtual Func<PacketConstructorInfo, IServerBoundPacket> CreatePacket => throw new Exception("Packet constructor not set");
}
