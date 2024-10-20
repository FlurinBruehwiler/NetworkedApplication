﻿using System.Net.Sockets;
using System.Runtime.InteropServices;
using MemoryPack;

namespace Shared;

public static class Networking
{
    public const int Version = 1;
    public static Dictionary<Guid, PendingFunction> PendingFunctions = [];

    public static unsafe Result SendMessage(TcpClient tcpClient, Span<byte> message)
    {
        try
        {
            var stream = tcpClient.GetStream();

            var length = message.Length;

            stream.Write(new Span<byte>(&length, 4));

            stream.Write(message);

        }
        catch (Exception e)
        {
            return Result.Error(e.ToString());
        }

        return Result.Success();
    }

    public static async ValueTask<byte[]> GetNextMessage(TcpClient tcpClient)
    {
        var stream = tcpClient.GetStream();

        byte[] header = new byte[4]; //todo pool

        await stream.ReadExactlyAsync(header);

        var length = ConvertToInt(header);

        byte[] message = new byte[length]; //todo pool

        await stream.ReadExactlyAsync(message);

        return message;
    }

    public static Task<TReturn> SendInvocation<TArg, TReturn>(TcpClient tcpClient, TArg args)
    {
        var paramsAsBytes = MemoryPackSerializer.Serialize(args);

        Span<byte> messageToSend = new byte[MessageHeader.Size + InvocationMessageHeader.Size + paramsAsBytes.Length];

        MessageDecoder.Write(ref messageToSend, new MessageHeader
        {
            MessageType = MessageType.FunctionInvocation,
        });

        var invocationGuid = Guid.NewGuid();

        MessageDecoder.Write(ref messageToSend, new InvocationMessageHeader
        {
            FunctionId = 1,
            InvocationGuid = invocationGuid
        });
        paramsAsBytes.CopyTo(messageToSend);
        SendMessage(tcpClient, messageToSend);

        var tcs = new TaskCompletionSource<TReturn>();

        PendingFunctions.Add(invocationGuid, new PendingFunction
        {
            ReturnType = typeof(TReturn),
            SetResult = returnValue => tcs.SetResult((TReturn)returnValue)
        });

        return tcs.Task;
    }

    public static void SendReturnValue<T>(TcpClient tcpClient, T returnValue, Guid invocationGuid)
    {
        var returnValueAsBytes = MemoryPackSerializer.Serialize(returnValue);

        Span<byte> messageToSend = new byte[MessageHeader.Size + ReturnMessageHeader.Size + + returnValueAsBytes.Length];

        MessageDecoder.Write(ref messageToSend, new MessageHeader
        {
            MessageType = MessageType.FunctionResponse
        });

        MessageDecoder.Write(ref messageToSend, new ReturnMessageHeader
        {
            InvocationGuid = invocationGuid
        });

        returnValueAsBytes.CopyTo(messageToSend);

        SendMessage(tcpClient, messageToSend);
    }

    private static unsafe int ConvertToInt(byte[] array)
    {
        fixed (byte* firstChar = array)
        {
            int* i = (int*)firstChar;
            return *i;
        }
    }
}

public struct PendingFunction
{
    public required Type ReturnType;
    public required Action<object> SetResult;
}