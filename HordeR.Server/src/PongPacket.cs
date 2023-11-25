namespace HordeR.Server;

public struct PongPacket
{
    public long Tick { get; }
    public long ClientTick { get; }

    public PongPacket(long tick, long clientTick)
    {
        Tick = tick;
        ClientTick = clientTick;
    }
}
