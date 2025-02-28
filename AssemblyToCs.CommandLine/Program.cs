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
    public struct InstructionTarget(int offset)
    {
        public int Offset = offset;
    }

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

        var parameters = new List<object>()
        {
            new MilRegister(0),
            new MilRegister(1),
        };

        var mil = new List<MilInstruction>()
        {
            new MilInstruction(0x0, MilOpCode.ShiftStack, 0x123),
            new MilInstruction(0x1, MilOpCode.Move, new MilRegister(2), new MilRegister(0)),
            new MilInstruction(0x2, MilOpCode.Add, new MilRegister(2), new MilRegister(1)),
            new MilInstruction(0x3, MilOpCode.Call, method2, new MilRegister(2), new MilRegister(2)),
            new MilInstruction(0x4, MilOpCode.ShiftStack, 0x28),
            new MilInstruction(0x5, MilOpCode.Return, new MilRegister(2)),
            // simplifier should remove these
            new MilInstruction(0x6, MilOpCode.Add, new MilMemoryLocation(2, 0x123), new MilStackOffset(0x123)),
            new MilInstruction(0x7, MilOpCode.Xor, new MilRegister(0), new MilRegister(0)),
            new MilInstruction(0x8, MilOpCode.Add, new MilMemoryLocation(2, 0x123), new MilStackOffset(0x123))

            /*new MilInstruction(0x0, MilOpCode.ShiftStack, -0x28),
            new MilInstruction(0x1, MilOpCode.CheckNotEqual, new MilRegister(500),
                new MilMemoryLocation(null, 0x57BBDF0), 0),
            new MilInstruction(0x2, MilOpCode.ConditionalJump, new InstructionTarget(0x6), new MilRegister(500)),
            new MilInstruction(0x3, MilOpCode.Move, new MilRegister(0), 0x5477120),
            new MilInstruction(0x4, MilOpCode.Call, method2, new MilMemoryLocation(null, 0x57BBDF0),
                new MilMemoryLocation(null, 0x57BBDF0)),
            new MilInstruction(0x5, MilOpCode.Move, new MilMemoryLocation(null, 0x57BBDF0), 1),
            new MilInstruction(0x6, MilOpCode.Move, new MilRegister(0), new MilMemoryLocation(null, 0x5477120)),
            new MilInstruction(0x7, MilOpCode.CheckNotEqual, new MilRegister(500), new MilMemoryLocation(0, 0x0e0), 0),
            new MilInstruction(0x8, MilOpCode.ConditionalJump, new InstructionTarget(0xb), new MilRegister(500)),
            new MilInstruction(0x9, MilOpCode.ShiftStack, 0x28),
            new MilInstruction(0xa, MilOpCode.Call, method2, new MilMemoryLocation(null, 0x57BBDF0),
                new MilMemoryLocation(null, 0x57BBDF0)),
            new MilInstruction(0xb, MilOpCode.ShiftStack, 0x28),
            new MilInstruction(0xc, MilOpCode.Return)*/

            /*new MilInstruction(0x0, MilOpCode.ShiftStack, 0x123),
            new MilInstruction(0x1, MilOpCode.Move, new MilStackOffset(0x10), 0),
            new MilInstruction(0x2, MilOpCode.ShiftStack, 0x123),
            new MilInstruction(0x3, MilOpCode.Return)*/
        };

        foreach (var instruction in mil)
        {
            if ((instruction.OpCode is MilOpCode.Jump or MilOpCode.ConditionalJump) &&
                instruction.Operands[0] is InstructionTarget)
            {
                var target = (InstructionTarget)instruction.Operands[0]!;
                var targetInstruction = mil.First(i => i.Offset == target.Offset);
                instruction.Operands[0] = targetInstruction;
            }
        }

        var decompilerMethod = new Method(method, mil, parameters);

        var decompiler = new Decompiler();

        decompiler.PreDecompile = (method3) => Console.WriteLine("    ----- PreDecompile");
        decompiler.PostSimplify = (method3) => Console.WriteLine("    ----- PostSimplify");
        decompiler.PostBuildCfg = (method3) => Console.WriteLine("    ----- PostBuildCfg");
        decompiler.PostSimplifyCfg = (method3) => Console.WriteLine("    ----- PostSimplifyCfg");
        decompiler.PostBuildDominance = (method3) => Console.WriteLine("    ----- PostBuildDominance");
        decompiler.PostDecompile = (method3) => Console.WriteLine("    ----- PostDecompile");

        decompiler.InfoLog = (text, source) => Console.WriteLine($"{source} : {text}");
        decompiler.WarnLog = (text, source) => Console.WriteLine($"{source} [Warn] : {text}");
        decompiler.ErrorLog = (text, source) => Console.WriteLine($"{source} [Error] : {text}");

        var workingDirectory = Path.GetDirectoryName(Environment.ProcessPath!)!;
        var assemblyPath = Path.Combine(workingDirectory, "TempAssembly.dll");
        module.Write(assemblyPath);

        Console.WriteLine($"Method: {method}");

        Console.WriteLine("MIL:");
        Console.WriteLine(string.Join(Environment.NewLine, mil.Select(i => "    " + i)));
        Console.WriteLine();

        var code = decompiler.DecompileAsString(decompilerMethod, workingDirectory);
        Console.WriteLine();
        var decompiledAssemblyPath = Path.Combine(workingDirectory, "TempAssembly_decompiled.dll");
        module.Write(decompiledAssemblyPath);

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
