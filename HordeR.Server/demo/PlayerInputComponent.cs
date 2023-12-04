namespace demo;

public class PlayerInputComponent
{
    private readonly Player player;

    public int Sequence { get; set; }

    public PlayerInputComponent(Player player)
    {
        this.player = player;
    }
}
