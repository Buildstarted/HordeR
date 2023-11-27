using HordeR.Server.Packets;

namespace HordeR.Server;

public struct PongPacket : IClientBoundPacket
{
    public string Type => nameof(PongPacket);
    public long Tick { get; }
    public long ClientTick { get; }

    public PongPacket(long tick, long clientTick)
    {
        Tick = tick;
        ClientTick = clientTick;
    }
}
