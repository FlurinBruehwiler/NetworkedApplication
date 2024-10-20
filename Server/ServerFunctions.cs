using Shared;

namespace Server;

public class ServerFunctions : IServerFunctions
{
    public Task<int> Execute(int param1, float param2)
    {
        Console.WriteLine($"Got {param1}, {param2}");
        return Task.FromResult(param1);
    }
}