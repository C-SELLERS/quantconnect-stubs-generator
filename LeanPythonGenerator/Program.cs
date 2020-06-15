﻿using System;
using System.IO;
using System.Linq;
using LeanPythonGenerator.Model;
using LeanPythonGenerator.Parse;
using LeanPythonGenerator.Render;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LeanPythonGenerator
{
    class Program
    {
        private readonly string _leanPath;
        private readonly string _outputDirectory;

        private Program(string leanDirectory, string outputDirectory)
        {
            _leanPath = FormatPath(leanDirectory);
            _outputDirectory = FormatPath(outputDirectory);
        }

        private void Run()
        {
            // Projects not to generate type hints for
            var blacklistedProjects = new[]
            {
                // Example projects
                "Algorithm.CSharp",
                "Algorithm.FSharp",
                "Algorithm.Python",
                "Algorithm.VisualBasic",

                // Other non-relevant projects
                "PythonToolbox",
                "Tests",
                "ToolBox"
            };

            // Path prefixes for all blacklistedProjects
            var blacklistedPrefixes = blacklistedProjects
                .Select(project => $"{_leanPath}/{project}/")
                .ToList();

            // Find all C# files in non-blacklisted projects
            var sourceFiles = Directory
                .EnumerateFiles(_leanPath, "*.cs", SearchOption.AllDirectories)
                .Where(file => !blacklistedPrefixes.Any(file.StartsWith))
                .ToList();

            Console.WriteLine($"Parsing {sourceFiles.Count} C# files");

            // Create syntax trees for all C# files
            var syntaxTrees = sourceFiles
                .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file))
                .ToList();

            // Create a compilation containing all syntax trees to retrieve semantic models containing type info from
            var compilation = CSharpCompilation
                .Create("")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(syntaxTrees);

            // Create an empty ParseContext which will be filled with all relevant information during parsing
            var context = new ParseContext();

            // Create utility classes
            // These classes shouldn't be imported, but are used to improve type hinting
            CreateUtilityClasses(context);

            // Parse all syntax trees
            foreach (var syntaxTree in syntaxTrees)
            {
                Console.WriteLine($"Parsing {syntaxTree.FilePath}");

                var model = compilation.GetSemanticModel(syntaxTree);

                var parser = new Parser(context, model);
                parser.Visit(syntaxTree.GetRoot());
            }

            // Render .pyi files containing type hints for all parsed namespaces
            foreach (var ns in context.GetNamespaces())
            {
                var relativePath = $"{ns.Name.Replace('.', '/')}/__init__.pyi";
                var pyiPath = Path.GetFullPath(relativePath, _outputDirectory);

                Console.WriteLine($"Generating {pyiPath}");

                new FileInfo(pyiPath).Directory?.Create();
                using var writer = new StreamWriter(pyiPath);

                var renderer = new NamespaceRenderer(writer, 0);
                renderer.Render(ns);
            }
        }

        private void CreateUtilityClasses(ParseContext context)
        {
            var ns = context.GetNamespaceByName("QuantConnect");

            CreateKeyValuePair(ns);
        }

        private void CreateKeyValuePair(Namespace ns)
        {
            var classType = new PythonType("KeyValuePair", ns.Name);

            var keyType = new PythonType($"{classType.Name}_K", ns.Name)
            {
                IsNamedTypeParameter = true
            };

            var valueType = new PythonType($"{classType.Name}_V", ns.Name)
            {
                IsNamedTypeParameter = true
            };

            classType.TypeParameters.Add(keyType);
            ns.TypeParameterNames.Add(keyType.Name);

            classType.TypeParameters.Add(valueType);
            ns.TypeParameterNames.Add(valueType.Name);

            Class cls = ns.GetClassByType(classType);
            cls.Summary = "This class exists to improve type hints. It cannot be imported.";

            // TODO(jmerle): Add Key and Value properties
        }

        private string FormatPath(string path)
        {
            var cwd = Directory.GetCurrentDirectory();
            var resolvedPath = Path.GetFullPath(path, cwd);

            var normalizedPath = resolvedPath.Replace('\\', '/');

            if (normalizedPath.EndsWith("/"))
            {
                normalizedPath = normalizedPath.Substring(0, path.Length - 1);
            }

            return normalizedPath;
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: dotnet run <Lean directory> <output directory>");
                Environment.Exit(1);
            }

            new Program(args[0], args[1]).Run();
        }
    }
}
