using System.Text.Json.Nodes;

namespace HordeR.Server;

public struct PacketConstructorInfo
{
    public Connection Connection { get; }
    public JsonNode Message { get; }

    public PacketConstructorInfo(Connection connection, JsonNode message)
    {
        Connection = connection;
        Message = message;
    }
}
