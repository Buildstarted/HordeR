using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HordeR.Server;

using PacketBuilder = Func<PacketConstructorInfo, IServerBoundPacket>;

public class ConnectionPacket : IServerBoundPacket
{
    public static PacketBuilder CreatePacket => (packet) => new ConnectionPacket(packet);
    public static int Type => (int)PacketType.Connect;

    public Connection Connection { get; }

    public ConnectionPacket(PacketConstructorInfo packet)
    {
        Connection = packet.Connection;
    }
}
