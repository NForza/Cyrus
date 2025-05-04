using NForza.Cyrus.TypeScriptGenerator;

var argumentParser = new ArgumentParser(args);

(var succeeded, var model) = new ModelGenerator(argumentParser.Path ?? string.Empty).GetModel();

if (!succeeded)
{
    Console.WriteLine("Failed to get model from assembly.");
    return 1;
}

Console.WriteLine(model);
return 0;
