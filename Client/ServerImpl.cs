using System.Net.Sockets;
using MemoryPack;
using Shared;

namespace Client;

public class ServerImpl : IServerFunctions
{
    public required TcpClient TcpClient;

    public ValueTask<int> Execute(int param1, float param2)
    {
        var args = new ExecuteArguments
        {
            Param1 = param1,
            Param2 = param2
        };

        return Networking.SendInvocation<ExecuteArguments, int>(TcpClient, args);
    }
}