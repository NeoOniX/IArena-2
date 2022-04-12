public class PlayerConfigFile
{
    public string name;
    public string color;
    public string control;
    public string destructor;
    public string interceptor;

    public PlayerConfigFile (PlayerConfig p)
    {
        name = p.name;
        color = p.color;
        control = p.control;
        destructor = p.destructor;
        interceptor = p.interceptor;
    }
}