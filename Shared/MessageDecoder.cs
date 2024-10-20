using System.Runtime.InteropServices;

namespace Shared;

[StructLayout(LayoutKind.Sequential)]
public partial struct InvocationMessageHeader
{
    public Guid InvocationGuid;
    public int FunctionId;

    public static unsafe int Size => sizeof(InvocationMessageHeader);
}

[StructLayout(LayoutKind.Sequential)]
public struct MessageHeader
{
    public MessageType MessageType;

    public static unsafe int Size => sizeof(MessageHeader);
}

[StructLayout(LayoutKind.Sequential)]
public struct ReturnMessageHeader
{
    public Guid InvocationGuid;

    public static unsafe int Size => sizeof(ReturnMessageHeader);
}

public enum MessageType
{
    FunctionInvocation,
    FunctionResponse
}


public static class MessageDecoder
{
    public static unsafe T Read<T>(ref Memory<byte> message) where T : unmanaged
    {
        fixed (byte* m = message.Span)
        {
            T header = *(T*)m;
            message = message.Slice(sizeof(T));
            return header;
        }
    }

    public static unsafe T Write<T>(ref Span<byte> message, T valueToWrite) where T : unmanaged
    {
        fixed (byte* m = message)
        {
            T* destination = (T*)m;
            *destination = valueToWrite;
            message = message.Slice(sizeof(T));
        }

        return valueToWrite;
    }
}