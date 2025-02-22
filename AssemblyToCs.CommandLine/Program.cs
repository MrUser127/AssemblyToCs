using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet;

namespace AssemblyToCs.CommandLine;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No args");
            PrintHelp();
            Environment.Exit(1);
        }

        var command = args[0];

        // decompile
        if (command == "dec")
        {
            var dllPath = GetArgValue("--dll");
            var assemblyDirectory = Path.GetDirectoryName(dllPath)!;
            var typeAndMethod = GetArgValue("--method").Split("::");
            var typeName = typeAndMethod[0];
            var methodName = typeAndMethod[1];

            var module = ModuleDefinition.FromFile(dllPath);
            var type = module.GetAllTypes().FirstOrDefault(type => type.FullName == typeName);

            if (type == null)
            {
                Console.WriteLine($"Could not find type {typeName}");
                Environment.Exit(1);
            }

            var method = type.Methods.FirstOrDefault(method => method.Name == methodName);

            if (method == null)
            {
                Console.WriteLine($"Could not find method {method}");
                Environment.Exit(1);
            }

            Console.WriteLine($"Method found: {method}");

            var decompiler = new Decompiler();

            decompiler.InfoLog = (text, source) => Console.WriteLine($"{source} : {text}");
            decompiler.WarnLog = (text, source) => Console.WriteLine($"{source} [Warn] : {text}");
            decompiler.ErrorLog = (text, source) => Console.WriteLine($"{source} [Error] : {text}");

            Console.WriteLine("Decompiling...");
            var code = decompiler.DecompileAsString(method, assemblyDirectory);

            Console.WriteLine("Decompiled code:");
            Console.WriteLine();
            Console.WriteLine(code);
        }
        else
        {
            Console.WriteLine($"Unknown command: {command}");
            PrintHelp();
            Environment.Exit(1);
        }

        return;

        string GetArgValue(string item)
        {
            var index = args.ToList().FindIndex(a => a == item);

            if (index == -1)
            {
                Console.WriteLine($"Argument {item} not found, add {item} [value]");
                PrintHelp();
                Environment.Exit(1);
            }

            if (args.Length <= index + 1)
            {
                Console.WriteLine($"No value for {item}, add something after it");
                PrintHelp();
                Environment.Exit(1);
            }

            return args[index + 1];
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine();
        Console.WriteLine("""
                          Syntax: command --arg value

                          Commands:
                              dec: decompile
                                  --dll: What dll to decompile method in?
                                  --method: What method to decompile? (Namespace.Type+NestedType::Method)
                          """);
    }
}
