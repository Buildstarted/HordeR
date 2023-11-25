using HordeR.Server;

namespace demo.Packets.ClientBound;

public class ChatMessagePacket : IClientBoundPacket
{
    public string Type => nameof(ChatMessagePacket);
    public string Name { get; }
    public string Message { get; }

    public ChatMessagePacket(string name, string message)
    {
        Name = name;
        Message = message;
    }
}
