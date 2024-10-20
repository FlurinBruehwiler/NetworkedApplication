using System.Net;
using System.Net.Sockets;
using Shared;

namespace Server;

public static class Program
{
    public static void Main()
    {
        var listener = new TcpListener(IPAddress.Any, 51234);
        listener.Start();

        Console.WriteLine("Listening for Clients");

        var serverFunctions = new ServerFunctions();

        while (true)
        {
            TcpClient tcpClient = listener.AcceptTcpClient();

            Console.WriteLine("Client connected");

            ListenForMessages(tcpClient, serverFunctions).ContinueWith(_ =>
            {
                Console.WriteLine("Client disconnected");
            });
        }
    }

    private static async Task ListenForMessages(TcpClient tcpClient, ServerFunctions serverFunctions)
    {
        Console.WriteLine("Listening for messages");

        while (tcpClient.Connected)
        {


        }

        Console.WriteLine("Stop listening for messages");
    }
}