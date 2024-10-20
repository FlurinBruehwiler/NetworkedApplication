
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceGenerator;

var source = """
             public interface IServerFunctions
             {
                 public ValueTask<int> Execute(int param1, float param2);
             }
             """;

var syntaxTree = CSharpSyntaxTree.ParseText(source);

var compilation = CSharpCompilation.Create(
    assemblyName: "Tests",
    syntaxTrees: [syntaxTree], [ ]);

var generator = new NetworkingGenerator();

GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

driver = driver.RunGenerators(compilation);

var result = driver.GetRunResult();
foreach (var tree in result.GeneratedTrees)
{
    var text = tree.GetText();
    Console.WriteLine(text);
}