using Shared;

namespace Client;

public struct SetNameArgs
{
    public int arg1;
    public float arg2;
}

public class ClientFunctions : IClientFunctions
{
    public bool SetName(int arg1, float arg2)
    {
        Console.WriteLine($"{arg1}, {arg2}");
        return true;
    }
}