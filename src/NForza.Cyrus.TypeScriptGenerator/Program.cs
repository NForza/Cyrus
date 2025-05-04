using Cyrus;
using NForza.Cyrus.TypeScriptGenerator;

var argumentParser = new ArgumentParser(args);

if (string.IsNullOrEmpty(argumentParser.Output) || string.IsNullOrEmpty(argumentParser.Path))
{
    Console.WriteLine("Usage: --output <output-path> --path <model-assembly-file>");
    return 1;
}

(var succeeded, var model) = new ModelGenerator(argumentParser.Path ?? string.Empty).GetModel();

if (!succeeded)
{
    Console.WriteLine("Failed to get model from assembly.");
    return 1;
}

TypeScriptGenerator.Generate(model!, argumentParser.Output);
return 0;
