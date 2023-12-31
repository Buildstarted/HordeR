﻿using HordeR.Server.Packets;

namespace demo.Packets.ClientBound;

public class EntityPositionPacket : IClientBoundPacket
{
    public string Type => nameof(EntityPositionPacket);

    public string Id { get; }
    public float X { get; }
    public float Y { get; }
    public int Sequence { get; }

    public EntityPositionPacket(string id, float x, float y, int sequence)
    {
        Id = id;
        X = x;
        Y = y;
        Sequence = sequence;
    }
}
