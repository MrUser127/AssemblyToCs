using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AssemblyToCs.MIL;

namespace AssemblyToCs.CommandLine;

internal class Program
{
    private static void Main(string[] args)
    {
        var module = new ModuleDefinition("TheAssembly");

        var importer = module.DefaultImporter;
        var corLibTypes = module.CorLibTypeFactory;

        var type = new TypeDefinition("TheNamespace", "TheClass", TypeAttributes.Class | TypeAttributes.Public);
        module.TopLevelTypes.Add(type);

        var signature = new MethodSignature(CallingConventionAttributes.Default, corLibTypes.Int32,
            new List<TypeSignature>()
            {
                corLibTypes.Int32,
                corLibTypes.Int32
            });
        var method = new MethodDefinition("TheMethod", MethodAttributes.Public, signature);
        method.ParameterDefinitions.Add(new ParameterDefinition(1, "A", 0));
        method.ParameterDefinitions.Add(new ParameterDefinition(2, "B", 0));
        type.Methods.Add(method);

        var parameters = new List<(object, OperandType)>()
        {
            (0, OperandType.Register),
            (1, OperandType.Register)
        };

        var il = new List<Instruction>()
        {
            new Instruction(0x0, OpCode.Move,
                (2, OperandType.Register),
                (0, OperandType.Register)
            ),
            new Instruction(0x1, OpCode.Add,
                (2, OperandType.Register),
                (1, OperandType.Register)
            ),
            new Instruction(0x3, OpCode.Return,
                (2, OperandType.Register)
            )
        };

        var workingDirectory = Path.GetDirectoryName(Environment.ProcessPath!)!;
        var assemblyPath = Path.Combine(workingDirectory, "TempAssembly.dll");
        module.Write(assemblyPath);

        Console.WriteLine($"Method: {method}");

        Console.WriteLine();
        Console.WriteLine("MIL:");
        Console.WriteLine(string.Join(Environment.NewLine, il));

        var decompiler = new Decompiler();

        decompiler.InfoLog = (text, source) => Console.WriteLine($"{source} : {text}");
        decompiler.WarnLog = (text, source) => Console.WriteLine($"{source} [Warn] : {text}");
        decompiler.ErrorLog = (text, source) => Console.WriteLine($"{source} [Error] : {text}");

        Console.WriteLine();
        Console.WriteLine("Decompiling...");
        var code = decompiler.DecompileAsString(method, il, parameters, workingDirectory);

        Console.WriteLine();
        Console.WriteLine("Decompiled code:");
        Console.WriteLine(code);
    }
}
