using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AssemblyToCs.Mil;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;

namespace AssemblyToCs.CommandLine;

internal class Program
{
    public struct InstructionTarget(int index)
    {
        public int Index = index;
    }

    private static void Main(string[] args)
    {
        var decompiler = new Decompiler();

        decompiler.InfoLog = (text, source) => Console.WriteLine($"{source} : {text}");
        decompiler.WarnLog = (text, source) => Console.WriteLine($"{source} [Warn] : {text}");
        decompiler.ErrorLog = (text, source) => Console.WriteLine($"{source} [Error] : {text}");

        var theAssembly = new ModuleDefinition("TheAssembly");

        var importer = theAssembly.DefaultImporter;
        var corLibTypes = theAssembly.CorLibTypeFactory;

        var theClass = new TypeDefinition("TheNamespace", "TheClass", TypeAttributes.Class | TypeAttributes.Public);
        theAssembly.TopLevelTypes.Add(theClass);

        var theMethodSignature = new MethodSignature(CallingConventionAttributes.Default, corLibTypes.Int32,
            new List<TypeSignature>()
            {
                corLibTypes.Int32,
                corLibTypes.Int32
            });
        var theMethod = new MethodDefinition("TheMethod", MethodAttributes.Public, theMethodSignature);
        theMethod.ParameterDefinitions.Add(new ParameterDefinition(1, "a", 0));
        theMethod.ParameterDefinitions.Add(new ParameterDefinition(2, "b", 0));
        theClass.Methods.Add(theMethod);

        var doSomethingSignature = new MethodSignature(CallingConventionAttributes.Default, corLibTypes.Int32,
            new List<TypeSignature>()
            {
                corLibTypes.Int32
            });
        var doSomething = new MethodDefinition("DoSomething", MethodAttributes.Public, doSomethingSignature);
        doSomething.ParameterDefinitions.Add(new ParameterDefinition(1, "num", 0));
        theClass.Methods.Add(doSomething);

        var workingDirectory = Path.GetDirectoryName(Environment.ProcessPath!)!;
        var assemblyPath = Path.Combine(workingDirectory, "TempAssembly.dll");
        theAssembly.Write(assemblyPath);

        var paramLocations = new List<object>()
        {
            new MilRegister(0),
            new MilRegister(1),
        };

        MilRegister Reg(int i) => new MilRegister(i);
        var b = new MilBuilder();

        b.Move(0, Reg(2), Reg(0));
        b.Add(0, Reg(2), Reg(1));
        b.Push(0, Reg(2), 8);
        b.Pop(0, Reg(3), 8);
        b.Call(0, doSomething, Reg(3), Reg(3));
        b.Push(0, Reg(3), 8);
        b.Pop(0, Reg(5), 8);
        b.Move(0, Reg(4), Reg(5));
        b.Return(0, Reg(4));

        b.FixBranches();
        b.FixIndexes();

        var instructions = b.Instructions;
        var decompilerMethod = new Method(theMethod, instructions, paramLocations, 8);

        Console.WriteLine($"Method: {theMethod}");

        Console.WriteLine("MIL:");
        Console.WriteLine(string.Join(Environment.NewLine, instructions.Select(i => "    " + i)));
        Console.WriteLine();

        var code = decompiler.DecompileAsString(decompilerMethod, workingDirectory);

        var decompiledAssemblyPath = Path.Combine(workingDirectory, "TempAssembly_decompiled.dll");
        theAssembly.Write(decompiledAssemblyPath);

        Console.WriteLine();
        Console.WriteLine("MIL:");
        Console.WriteLine(string.Join(Environment.NewLine, decompilerMethod.Instructions.Select(i => "    " + i)));

        Console.WriteLine();
        Console.WriteLine(BuildGraph(decompilerMethod));

        Console.WriteLine(code);
    }

    private static string BuildGraph(Method method)
    {
        var cfg = method.FlowGraph!;

        var directedGraph = new DotGraph()
            .WithIdentifier("ControlFlowGraph")
            .Directed()
            .WithLabel("Control flow graph");

        var nodes = new Dictionary<int, DotNode>();
        var edges = new List<DotEdge>();

        foreach (var block in cfg.Blocks)
        {
            var node = GetOrAddNode(block.Id);

            if (block == cfg.EntryBlock)
            {
                node.WithLabel($"Entry ({block.Id})");
                node.WithColor("green");
            }
            else if (block == cfg.ExitBlock)
            {
                node.WithLabel($"Exit ({block.Id})");
                node.WithColor("red");
            }
            else
            {
                node.WithShape("box");

                var sb = new StringBuilder();
                sb.AppendLine("Block " + block.Id);
                if (method.Dominance!.DominanceTree.TryGetValue(block, out var doms))
                    sb.AppendLine($"Immediate dominators: {string.Join(", ", doms.Select(d => d.Id))}");
                sb.AppendLine();
                sb.Append(string.Join(Environment.NewLine, block.Instructions));
                node.WithLabel(sb.ToString());
            }

            foreach (var successor in block.Successors)
                GetOrAddEdge(node, GetOrAddNode(successor.Id));
        }

        using var writer = new StringWriter();
        var context = new CompilationContext(writer, new CompilationOptions());
        directedGraph.CompileAsync(context);
        var result = writer.GetStringBuilder().ToString();
        return result;

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
