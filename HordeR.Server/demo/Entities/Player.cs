using HordeR.Server;
using System.Text.Json.Serialization;

public abstract class Entity
{
    public string Id { get; }
    public float X { get; set; }
    public float Y { get; set; }
    public string Color { get; set; }

    public Entity()
    {
        Id = Guid.NewGuid().ShortGuid();
    }
}

public class Player : Entity
{
    [JsonIgnore]
    public Connection Connection { get; }

    public Player(Connection connection)
    {
        Connection = connection;
        Color = Random.Shared.Next(0, 360).ToString();
    }

    public void Tick()
    {

    }

    internal void HandleInput(InputPacket packet)
    {
        if(packet.Up)
        {
            Y -= 10;
        }
        if(packet.Down)
        {
            Y += 10;
        }
        if(packet.Left)
        {
            X -= 10;
        }
        if(packet.Right)
        {
            X += 10;
        }
    }
}
