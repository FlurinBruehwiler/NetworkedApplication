using System.Text;

namespace SourceGenerator;

public sealed class CodeBuilder : IDisposable
{
    private int indentLevel = 0;
    private readonly StringBuilder sb = new();

    public void AppendLine(string s)
    {
        for (var i = 0; i < indentLevel; i++)
            sb.Append("    ");

        sb.Append(s);
        sb.Append('\n');
    }

    public IDisposable CodeBlock()
    {
        AppendLine("{");
        indentLevel++;
        return this;
    }

    public void Dispose()
    {
        indentLevel--;
        AppendLine("}");
    }

    public override string ToString() => sb.ToString();
}