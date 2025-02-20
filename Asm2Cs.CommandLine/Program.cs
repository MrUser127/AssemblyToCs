using System;
using System.Collections.Generic;
using System.IO;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using Asm2Cs.IL;

namespace Asm2Cs.CommandLine;

internal class Program
{
    private static void Main(string[] args)
    {
        List<ILInstruction> instructions = new List<ILInstruction>();

        instructions.Add(new ILInstruction(0, ILOpCode.Move,
            (new GlobalVariableOperand("something"), OperandType.GlobalVariable),
            (new ILInstruction(1, ILOpCode.Add,
                (0x1, OperandType.Int),
                (0.5f, OperandType.Float)
            ), OperandType.InstructionResult)
        ));
        instructions.Add(new ILInstruction(2, ILOpCode.Unknown, ("some unknown instruction", OperandType.String)));
        instructions.Add(new ILInstruction(3, ILOpCode.Call,
            (new GlobalVariableOperand("SomeFunction"), OperandType.GlobalVariable),
            (new GlobalVariableOperand("something"), OperandType.GlobalVariable)
        ));
        instructions.Add(new ILInstruction(6, ILOpCode.Xor,
            (0, OperandType.Register),
            (1, OperandType.Register))
        );
        instructions.Add(new ILInstruction(7, ILOpCode.Xor,
            (1, OperandType.Register),
            (1, OperandType.Register))
        );
        instructions.Add(new ILInstruction(4, ILOpCode.ConditionalJump,
            (instructions[1], OperandType.Instruction),
            (5, OperandType.Register)
        ));
        instructions.Add(new ILInstruction(5, ILOpCode.Return,
            (new GlobalVariableOperand("something"), OperandType.GlobalVariable)
        ));

        Function function = new Function("SomeFunction", instructions,
            new List<LocalVariable>()
            {
                new LocalVariable("param1", (1, OperandType.Register), DataType.Int),
                new LocalVariable("param2", (0x3, OperandType.StackVariable), DataType.Int),
            }, DataType.Int);

        var decompiler = new Decompiler();

        decompiler.InfoLog += (message, source) => Console.WriteLine($"{source} : {message}");
        decompiler.WarnLog += (message, source) => Console.WriteLine($"{source} [Warn] : {message}");
        decompiler.ErrorLog += (message, source) => Console.WriteLine($"{source} [Error] : {message}");

        var code = decompiler.DecompileFunctionAsString(function);

        Console.WriteLine();
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
