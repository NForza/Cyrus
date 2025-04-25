using System;
using NForza.Cyrus.TypescriptGenerate;

using System.CommandLine;

var inputFileOption = new Option<string>(
    name: "--input",
    description: "Path to model JSON file",
    getDefaultValue: () => "metadata.json");

var outputPathOption = new Option<string>(
    name: "--output",
    description: "TypeScript output folder",
    getDefaultValue: () => ".");

var root = new RootCommand("Generate")
{
    inputFileOption,
    outputPathOption            
};
root.SetHandler((input, output) => TypeScriptGenerator.Generate(input, output), inputFileOption, outputPathOption);

return await root.InvokeAsync(args); 