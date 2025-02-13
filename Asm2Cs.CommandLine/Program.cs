using System;
using System.Collections.Generic;

namespace Asm2Cs.CommandLine;

internal class Program
{
    private static void Main(string[] args)
    {
        List<ILInstruction> instructions = new List<ILInstruction>
        {
            new ILInstruction(0, ILOpCode.Store,
                new IntegerOperand(0x123),
                new ILInstruction(1, ILOpCode.Add,
                    new IntegerOperand(0x1),
                    new FloatOperand(0.1f)
                )
            )
        };

        Function function = new Function("SomeFunction", instructions);

        Console.WriteLine(function);
    }
}
