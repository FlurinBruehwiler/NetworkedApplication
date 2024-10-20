using MemoryPack;

namespace Shared;

[MemoryPackable]
public partial struct ExecuteArguments
{
    public int Param1;
    public float Param2;
}

public interface IServerFunctions
{
    public ValueTask<int> Execute(int param1, float param2);
}