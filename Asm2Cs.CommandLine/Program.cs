using System;
using System.Collections.Generic;
using System.IO;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;

namespace Asm2Cs.CommandLine;

internal class Program
{
    private static void Main(string[] args)
    {
        DataTypeManager dataTypeManager = new DataTypeManager();

        List<ILInstruction> instructions = new List<ILInstruction>();

        instructions.Add(new ILInstruction(0, ILOpCode.Move,
            new GlobalVariableOperand("something"),
            new ILInstruction(1, ILOpCode.Add,
                new IntegerOperand(0x1),
                new FloatOperand(0.5f)
            )
        ));
        instructions.Add(new ILInstruction(2, ILOpCode.Unknown));
        instructions.Add(new ILInstruction(3, ILOpCode.Call,
            new GlobalVariableOperand("SomeFunction"))
        );
        instructions.Add(new ILInstruction(4, ILOpCode.ConditionalJump,
            new BranchTarget(instructions[1]),
            new RegisterOperand(5)
        ));
        instructions.Add(new ILInstruction(5, ILOpCode.Return,
            new GlobalVariableOperand("something"))
        );

        Function function = new Function("SomeFunction", instructions,
            new List<LocalVariable>()
            {
                new LocalVariable("param1", new RegisterOperand(1), dataTypeManager.IntType),
                new LocalVariable("param2", new StackVariableOperand(0x3), dataTypeManager.IntType),
            }, new LocalVariable("<return>", new RegisterOperand(0), dataTypeManager.IntType), dataTypeManager);

        ControlFlowGraph graph = new ControlFlowGraph(instructions);
        WriteGraph(graph, Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "ControlFlowGraph.dot"));

        Console.WriteLine(function);
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
            var node = GetOrAddNode(block.ID);

            if (block == graph.EntryBlock)
                node.WithColor("green");

            node.WithShape("box");
            node.WithLabel(block.ToString() == "" ? "Entry" : ($"Block {block.ID}\n" + block));

            foreach (var successor in block.Successors)
                GetOrAddEdge(node, GetOrAddNode(successor.ID));
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
