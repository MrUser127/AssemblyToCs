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
            Console.WriteLine("No command line arguments");
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

            Console.WriteLine($"Method: {method}");

            var decompiler = new Decompiler();

            Console.WriteLine();
            Console.WriteLine(decompiler.DecompileAsString(method, assemblyDirectory));
        }
        else
        {
            Console.WriteLine($"Invalid command: {command}");
            PrintHelp();
            Environment.Exit(1);
        }

        return;

        string GetArgValue(string item)
        {
            var index = args.ToList().FindIndex(a => a == item);

            if (index == -1)
            {
                Console.WriteLine($"Argument {item} not found");
                PrintHelp();
                Environment.Exit(1);
            }

            if (args.Length <= index + 1)
            {
                Console.WriteLine($"No value for {item}");
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
                                  --dll is path to dll file to decompile method in
                                  --method is name of the method to decompile (Namespace.Type+NestedType::Method)
                          """);
    }
}
