using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AssemblyToCs.IL;

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

        var signature = new MethodSignature(CallingConventionAttributes.Default, corLibTypes.Void,
            new List<TypeSignature>()
            {
            });
        var methodDef = new MethodDefinition("TheMethod", MethodAttributes.Public, signature);
        type.Methods.Add(methodDef);

        var workingDirectory = Path.GetDirectoryName(Environment.ProcessPath!)!;
        var assemblyPath = Path.Combine(workingDirectory, "TempAssembly.dll");
        module.Write(assemblyPath);

        Console.WriteLine($"Method: {methodDef}");

        var method = new Method(methodDef, new List<ILInstruction>()
        {
            new ILInstruction(0x0, ILOpCode.Return,
                (new ILInstruction(0x1, ILOpCode.Add,
                        (1, OperandType.Int),
                        (2, OperandType.Int)
                    ),
                    OperandType.InstructionResult))
        });

        Console.WriteLine();
        Console.WriteLine("IL:");
        Console.WriteLine(string.Join(Environment.NewLine, method.Instructions));

        var decompiler = new Decompiler();

        decompiler.InfoLog = (text, source) => Console.WriteLine($"{source} : {text}");
        decompiler.WarnLog = (text, source) => Console.WriteLine($"{source} [Warn] : {text}");
        decompiler.ErrorLog = (text, source) => Console.WriteLine($"{source} [Error] : {text}");

        Console.WriteLine();
        Console.WriteLine("Decompiling...");
        var code = decompiler.DecompileAsString(methodDef, workingDirectory);

        Console.WriteLine();
        Console.WriteLine("Decompiled code:");
        Console.WriteLine(code);
    }
}
