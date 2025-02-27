using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AssemblyToCs.Mil;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;

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
        method.ParameterDefinitions.Add(new ParameterDefinition(1, "a", 0));
        method.ParameterDefinitions.Add(new ParameterDefinition(2, "b", 0));
        type.Methods.Add(method);

        var signature2 = new MethodSignature(CallingConventionAttributes.Default, corLibTypes.Int32,
            new List<TypeSignature>()
            {
                corLibTypes.Int32
            });
        var method2 = new MethodDefinition("DoSomething", MethodAttributes.Public, signature2);
        method2.ParameterDefinitions.Add(new ParameterDefinition(1, "num", 0));
        type.Methods.Add(method2);

        var parameters = new List<(object, MilOperand)>()
        {
            (0, MilOperand.Register),
            (1, MilOperand.Register)
        };

        var mil = new List<MilInstruction>()
        {
            new MilInstruction(0x0, MilOpCode.Move, (2, MilOperand.Register), (0, MilOperand.Register)),
            new MilInstruction(0x1, MilOpCode.Add, (2, MilOperand.Register), (1, MilOperand.Register)),
            new MilInstruction(0x2, MilOpCode.Call, (method2, MilOperand.Method), (2, MilOperand.Register),
                (2, MilOperand.Register)),
            new MilInstruction(0x3, MilOpCode.Return, (2, MilOperand.Register)),
            // unreachable
            new MilInstruction(0x4, MilOpCode.Move, (2, MilOperand.Register), (0, MilOperand.Register))
        };

        /*var mil = new List<MilInstruction>();
        mil.Add(new MilInstruction(0x0, MilOpCode.Move, (0, MilOperand.Register), (1, MilOperand.Register)));
        mil.Add(new MilInstruction(0x1, MilOpCode.Move, (4, MilOperand.Register), (1, MilOperand.Register)));
        mil.Add(new MilInstruction(0x2, MilOpCode.CheckNotSign, (500, MilOperand.Register), (0, MilOperand.Register),
            (0, MilOperand.Int)));
        mil.Add(new MilInstruction(0x3, MilOpCode.ConditionalJump, (0, MilOperand.None), (500, MilOperand.Register)));
        mil.Add(new MilInstruction(0x4, MilOpCode.Move, (3, MilOperand.Register), (0, MilOperand.Int)));
        mil.Add(new MilInstruction(0x5, MilOpCode.Jump, (0, MilOperand.None)));
        mil.Add(new MilInstruction(0x6, MilOpCode.CheckNotSign, (500, MilOperand.Register), (4, MilOperand.Register),
            (0, MilOperand.Int)));
        mil.Add(new MilInstruction(0x7, MilOpCode.ConditionalJump, (0, MilOperand.None), (500, MilOperand.Register)));
        mil.Add(new MilInstruction(0x8, MilOpCode.Move, (3, MilOperand.Register), (-1, MilOperand.Int)));
        mil.Add(new MilInstruction(0x9, MilOpCode.Jump, (0, MilOperand.None)));
        mil.Add(new MilInstruction(0x10, MilOpCode.Move, (5, MilOperand.Register), (0, MilOperand.Register)));
        mil.Add(new MilInstruction(0x11, MilOpCode.Move, (3, MilOperand.Register), (4, MilOperand.Register)));
        mil.Add(new MilInstruction(0x12, MilOpCode.Add, (3, MilOperand.Register), (5, MilOperand.Register)));
        mil.Add(new MilInstruction(0x13, MilOpCode.Return, (3, MilOperand.Register)));

        mil[3].Operands[0] = (mil[6], MilOperand.Branch);
        mil[5].Operands[0] = (mil[13], MilOperand.Branch);
        mil[7].Operands[0] = (mil[10], MilOperand.Branch);
        mil[9].Operands[0] = (mil[13], MilOperand.Branch);*/

        var decompilerMethod = new Method(method, mil, parameters);

        var workingDirectory = Path.GetDirectoryName(Environment.ProcessPath!)!;
        var assemblyPath = Path.Combine(workingDirectory, "TempAssembly.dll");
        module.Write(assemblyPath);

        Console.WriteLine($"Method: {method}");

        Console.WriteLine();
        Console.WriteLine("MIL:");
        Console.WriteLine(string.Join(Environment.NewLine, mil));

        //var cfg = ControlFlowGraph.Build(decompilerMethod);
        //WriteGraph(cfg, Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "Cfg.dot"));

        var decompiler = new Decompiler();

        decompiler.PreDecompile = (_) => Console.WriteLine("    PreDecompile invoked");
        decompiler.PostSimplify = (_) => Console.WriteLine("    PostSimplify invoked");
        decompiler.PostBuildCfg = (_) => Console.WriteLine("    PostBuildCfg invoked");
        decompiler.PostSimplifyCfg = (_) => Console.WriteLine("    PostSimplifyCfg invoked");
        decompiler.PostDecompile = (_) => Console.WriteLine("    PostDecompile invoked");

        decompiler.InfoLog = (text, source) => Console.WriteLine($"{source} : {text}");
        decompiler.WarnLog = (text, source) => Console.WriteLine($"{source} [Warn] : {text}");
        decompiler.ErrorLog = (text, source) => Console.WriteLine($"{source} [Error] : {text}");

        Console.WriteLine();
        Console.WriteLine("Decompiling...");
        var code = decompiler.DecompileAsString(decompilerMethod, workingDirectory);

        var decompiledAssemblyPath = Path.Combine(workingDirectory, "TempAssembly_decompiled.dll");
        module.Write(decompiledAssemblyPath);

        Console.WriteLine();
        Console.WriteLine("Decompiled code:");
        Console.WriteLine(code);
    }

    private static void WriteGraph(ControlFlowGraph graph, string path)
    {
        var directedGraph = new DotGraph()
            .WithIdentifier("ControlFlowGraph")
            .Directed()
            .WithLabel("Control flow graph");

        var nodes = new Dictionary<int, DotNode>();
        var edges = new List<DotEdge>();

        foreach (var block in graph.Blocks)
        {
            var node = GetOrAddNode(block.Id);

            if (block == graph.EntryBlock)
                node.WithColor("green");

            node.WithShape("box");
            node.WithLabel(block.Instructions.Count == 0 ? "Entry" : $"{block.Id}\n\n{block}");

            foreach (var successor in block.Successors)
                GetOrAddEdge(node, GetOrAddNode(successor.Id));
        }

        using var writer = new StringWriter();
        var context = new CompilationContext(writer, new CompilationOptions());
        directedGraph.CompileAsync(context);
        var result = writer.GetStringBuilder().ToString();
        File.WriteAllText(path, result);

        return;

        DotEdge GetOrAddEdge(DotNode from, DotNode to)
        {
            foreach (var edge in edges)
            {
                if (edge.From == from.Identifier && edge.To == to.Identifier)
                {
                    return edge;
                }
            }

            var newEdge = new DotEdge().From(from).To(to);
            edges.Add(newEdge);
            directedGraph.Add(newEdge);
            return newEdge;
        }

        DotNode GetOrAddNode(int id)
        {
            if (nodes.TryGetValue(id, out var node))
            {
                return node;
            }

            var newNode = new DotNode().WithIdentifier(id.ToString());
            directedGraph.Add(newNode);
            nodes[id] = newNode;
            return newNode;
        }
    }
}
