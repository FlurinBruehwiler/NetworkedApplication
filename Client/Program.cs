using System.Net;
using System.Net.Sockets;
using MemoryPack;
using Shared;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        var ip = IPAddress.Parse("127.0.0.1");

        var ipEndpoint = new IPEndPoint(ip, 5123);

        var tcpClient = new TcpClient();

        try
        {
            tcpClient.Connect(ipEndpoint);
            Console.WriteLine("Connected to server");
        }
        catch
        {
            Console.WriteLine("Server not available");
            return;
        }

        new Thread(() => ReceiveMessageThread(tcpClient, new ClientFunctions()).GetAwaiter().GetResult()).Start();

        var serverImpl = new IServerFunctionsImpl
        {
            TcpClient = tcpClient
        };
        int counter = 0;
        while (true)
        {
            counter++;
            var res = await serverImpl.Execute(counter, counter * 2.2f);
            Console.WriteLine($"Response: {res}");
        }
    }

    private static async Task ReceiveMessageThread(TcpClient tcpClient, ClientFunctions clientFunctions)
    {
        while (tcpClient.Connected)
        {
            await Networking.ProcessMessage(tcpClient, (header, memory) =>
            {
                switch (header.FunctionId)
                {
                    case 1:
                        var args = MemoryPackSerializer.Deserialize<SetNameArgs>(memory.Span);
                        var returnValue = clientFunctions.SetName(args.arg1, args.arg2);

                        Networking.SendReturnValue(tcpClient, returnValue, header.InvocationGuid);
                        break;
                }

                return ValueTask.CompletedTask;
            });
        }
    }
}