﻿using System.Net;
using System.Net.Sockets;
using MemoryPack;
using Shared;

namespace Server;

public static class Program
{
    public static void Main()
    {
        var listener = new TcpListener(IPAddress.Any, 5123);
        listener.Start();

        Console.WriteLine("Listening for Clients");

        var serverFunctions = new ServerFunctions();

        while (true)
        {
            TcpClient tcpClient = listener.AcceptTcpClient();

            Console.WriteLine("Client connected");

            ListenForMessages(tcpClient, serverFunctions).ContinueWith(e =>
            {
                if (e.Exception != null)
                {
                    Console.WriteLine(e.Exception.ToString());
                }
                Console.WriteLine("Client disconnected");
            });
        }
    }

    private static async Task ListenForMessages(TcpClient tcpClient, ServerFunctions serverFunctions)
    {
        Console.WriteLine("Listening for messages");

        while (tcpClient.Connected)
        {
            await Networking.ProcessMessage(tcpClient, async (header, memory) =>
            {
                switch (header.FunctionId)
                {
                    case 1:
                        var args = MemoryPackSerializer.Deserialize<ExecuteArguments>(memory.Span);
                        var returnValue = await serverFunctions.Execute(args.Param1, args.Param2);

                        Networking.SendReturnValue(tcpClient, returnValue, header.InvocationGuid);
                        break;
                }
            });
        }

        Console.WriteLine("Stop listening for messages");
    }
}