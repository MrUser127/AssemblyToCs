using System;
using System.Collections.Generic;

namespace Asm2Cs.CommandLine;

internal class Program
{
    private static void Main(string[] args)
    {
        DataTypeManager dataTypeManager = new DataTypeManager();

        List<ILInstruction> instructions = new List<ILInstruction>
        {
            new ILInstruction(0, ILOpCode.Move,
                new GlobalVariableOperand("something"),
                new ILInstruction(1, ILOpCode.Add,
                    new IntegerOperand(0x1),
                    new FloatOperand(0.5f)
                )
            ),
            new ILInstruction(2, ILOpCode.Return, new GlobalVariableOperand("something"))
        };

        Function function = new Function("SomeFunction", instructions,
            new List<LocalVariable>()
            {
                new LocalVariable("param1", new RegisterOperand(1), dataTypeManager.IntType),
                new LocalVariable("param2", new StackVariableOperand(0x3), dataTypeManager.IntType),
            });

        Console.WriteLine(function);
    }
}
