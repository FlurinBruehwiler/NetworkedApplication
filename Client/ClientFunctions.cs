using MemoryPack;
using Shared;

namespace Client;

[MemoryPackable]
public partial struct SetNameArgs
{
    public int arg1;
    public float arg2;
}

public class ClientFunctions : IClientFunctions
{
    public ValueTask<bool> SetName(int arg1, float arg2)
    {
        Console.WriteLine($"{arg1}, {arg2}");
        return ValueTask.FromResult(true);
    }
}