using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator;

[Generator]
public class NetworkingGenerator : IIncrementalGenerator
{
    SymbolDisplayFormat symbolDisplayFormat = new SymbolDisplayFormat(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions:SymbolDisplayGenericsOptions.IncludeTypeParameters);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var toGenerate = context.SyntaxProvider.CreateSyntaxProvider(predicate: Filter, transform: Transform);

        context.RegisterSourceOutput(toGenerate, Generate);
    }

    private void Generate(SourceProductionContext ctx, (INamedTypeSymbol, GeneratorSyntaxContext)? data)
    {
        if (data == null)
            return;

        var networkInterface = data.Value.Item1;

        var methods = networkInterface.GetMembers();

        var codeBuilder = new CodeBuilder();

        codeBuilder.AppendLine($"public class {networkInterface.Name}Impl : {networkInterface.ToDisplayString(symbolDisplayFormat)}");
        using (codeBuilder.CodeBlock())
        {
            codeBuilder.AppendLine("public required System.Net.Sockets.TcpClient TcpClient;");

            foreach (var method in methods)
            {
                if (method is not IMethodSymbol methodSymbol)
                    continue;

                var argStructName = $"{methodSymbol.Name}Args";
                
                //args struct
                codeBuilder.AppendLine($"struct {argStructName}");
                using (codeBuilder.CodeBlock())
                {
                    foreach (var parameter in methodSymbol.Parameters)
                    {
                        codeBuilder.AppendLine($"public {parameter.Type.ToDisplayString(symbolDisplayFormat)} {parameter.Name};");
                    }
                }

                //method implementation
                codeBuilder.AppendLine($"public {methodSymbol.ReturnType.ToDisplayString(symbolDisplayFormat)} {methodSymbol.Name}(");
                bool isFirst = true;
                foreach (var parameter in methodSymbol.Parameters)
                {
                    if (!isFirst)
                    {
                        codeBuilder.AppendLine(",");
                    }
                    isFirst = false;
                    codeBuilder.AppendLine($"{parameter.Type.ToDisplayString(symbolDisplayFormat)} {parameter.Name}");
                }
                codeBuilder.AppendLine(")");
                using (codeBuilder.CodeBlock()) //function impl
                {
                    codeBuilder.AppendLine($"var args = new {argStructName}");
                    using (codeBuilder.CodeBlock()) //struct initializer syntax
                    {
                        foreach (var parameter in methodSymbol.Parameters)
                        {
                            codeBuilder.AppendLine($"{parameter.Name} = {parameter.Name},");
                        }
                    }
                    codeBuilder.AppendLine(";");
                    if (methodSymbol.ReturnType is INamedTypeSymbol returnType)
                    {
                        var arg = returnType.TypeArguments.First();
                        codeBuilder.AppendLine($"return Shared.Networking.SendInvocation<{argStructName}, {arg.ToDisplayString(symbolDisplayFormat)}>(TcpClient, args);");
                    }
                }
            }
        }

        ctx.AddSource($"{networkInterface.Name}Impl.g.cs", SourceText.From(codeBuilder.ToString(), Encoding.UTF8));
    }

    private (INamedTypeSymbol, GeneratorSyntaxContext)? Transform(GeneratorSyntaxContext syntaxContext,
        CancellationToken token)
    {
        var classDeclarationSyntax = (InterfaceDeclarationSyntax)syntaxContext.Node;

        var symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

        if (symbol is not INamedTypeSymbol namedTypeSymbol)
            return null;

        return (namedTypeSymbol, syntaxContext);
    }

    private bool Filter(SyntaxNode syntaxNode, CancellationToken token)
    {

        if (syntaxNode is not InterfaceDeclarationSyntax classDeclarationSyntax)
        {
            return false;
        }

        return classDeclarationSyntax.Identifier.Text is "IServerFunctions" or "IClientFunctions";
    }
}