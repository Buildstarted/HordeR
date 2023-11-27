using HordeR.Server;
using HordeR.Server.Packets;
using PacketBuilder = System.Func<HordeR.Server.PacketConstructorInfo, HordeR.Server.Packets.IServerBoundPacket>;

public class InputPacket : IServerBoundPacket
{
    public static PacketBuilder CreatePacket => (packet) => new InputPacket(packet);
    public static int Type => PacketTypes.InputPacket;

    public Connection Connection { get; }
    public bool Up { get; }
    public bool Left { get; }
    public bool Down { get; }
    public bool Right { get; }
    public int Sequence { get; }

    public InputPacket(PacketConstructorInfo packet)
    {
        Connection = packet.Connection;
        var keys = packet.Message["i"]?.GetValue<int>() ?? 0;
        var up =    (keys & (1 << 0)) != 0;
        var left =  (keys & (1 << 1)) != 0;
        var down =  (keys & (1 << 2)) != 0;
        var right = (keys & (1 << 3)) != 0;
        var sequence = packet.Message["s"]?.GetValue<int>() ?? 0;

        Up = up;
        Left = left;
        Down = down;
        Right = right;
        Sequence = sequence;
    }
}
