using HordeR.Server;

namespace demo.Packets.ClientBound;

public class ChatMessageSendPacket : IClientBoundPacket
{
    public string Type => nameof(ChatMessageSendPacket);
    public string Name { get; }
    public string Message { get; }

    public ChatMessageSendPacket(string name, string message)
    {
        Name = name;
        Message = message;
    }
}
