using HordeR.Server;

namespace demo.Packets.ServerBound;

using PacketBuilder = System.Func<PacketConstructorInfo, IServerBoundPacket>;

public struct ChatMessagePacket : IServerBoundPacket
{
    public static PacketBuilder CreatePacket => (packet) => new ChatMessagePacket(packet);
    public static int Type => PacketTypes.ChatMessagePacket;
    public Connection Connection { get; }
    public string Message { get; }

    public ChatMessagePacket(PacketConstructorInfo packet)
    {
        Connection = packet.Connection;
        Message = packet.Message["message"]?.GetValue<string>() ?? "";
    }
}
