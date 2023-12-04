namespace HordeR.Server.Packets;
internal struct TickPacket : IServerBoundPacket
{
    public static int Type => (int)PacketType.Tick;

    public long WorldTick { get; }

    public TickPacket(long worldTick)
    {
        WorldTick = worldTick;
    }
}
