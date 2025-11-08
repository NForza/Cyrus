using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.TypeScriptGenerator.Tests
{
    internal class TypeScriptGeneratorTest
    {
        internal static TypeScriptGeneratorTestResult For(CyrusMetadata metadata)
        {
            var mockFileSystem = new MockFileSystem();
            TypeScriptGenerator.Generate(mockFileSystem, JsonSerializer.Serialize(metadata, ModelSerializerOptions.Default), "/");
            return new TypeScriptGeneratorTestResult(mockFileSystem);
        }
    }
}