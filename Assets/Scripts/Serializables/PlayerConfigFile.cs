public class PlayerConfigFile
{
    public string name;
    public string color;
    public string source;

    public PlayerConfigFile (PlayerConfig p)
    {
        name = p.name;
        color = p.color;
        source = p.source;
    }
}