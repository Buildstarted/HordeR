﻿using HordeR.Server;
using HordeR.Server.Packets;

namespace demo.Packets.ServerBound;

using PacketBuilder = Func<PacketConstructorInfo, IServerBoundPacket>;

public class JoinPacket : IServerBoundPacket
{
    public static PacketBuilder CreatePacket => (packet) => new JoinPacket(packet);
    public static int Type => PacketTypes.JoinPacket;

    public Connection Connection { get; }
    public string Name { get; }

    public JoinPacket(PacketConstructorInfo packet)
    {
        Connection = packet.Connection;
        Name = packet.Message["name"].GetValue<string>();
    }
}
