using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AssemblyToCs.Mil;

namespace AssemblyToCs;

/// <summary>
/// Translates medium level IL to .NET's IL.
/// </summary>
public class MilToCilTranslator
{
    private Dictionary<MilLocalVariable, CilLocalVariable> _locals = [];
    private Method _method;
    private CilMethodBody _methodBody;

    private ReferenceImporter _importer;
    private MethodDefinition _exceptionConstructor;

    /// <summary>
    /// Translates MIL of the method to .NET's IL and replaces the body with it.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="decompiler">The decompiler.</param>
    /// <param name="corLibTypes">corlib type factory.</param>
    public void Translate(Method method, Decompiler decompiler, CorLibTypeFactory corLibTypes)
    {
        decompiler.Info("Translating MIL -> CIL...", "MIL to CIL Translator");

        _method = method;
        var definition = method.Definition;
        GetExceptionConstructor(definition); // for decompilation errors

        _methodBody = new CilMethodBody(definition)
        {
            InitializeLocals =
                true // without this ilspy does System.Runtime.CompilerServices.Unsafe.SkipInit(out object obj);
        };

        definition.CilMethodBody = _methodBody;

        _locals = [];

        // add locals to cil method
        foreach (var local in method.Locals)
        {
            var cilLocal = new CilLocalVariable(local.Type!);
            _methodBody.LocalVariables.Add(cilLocal);
            _locals.Add(local, cilLocal);
        }

        // translate it
        foreach (var instruction in method.Instructions)
            TranslateInstruction(instruction);
    }

    // for decompilation errors
    private void GetExceptionConstructor(MethodDefinition definition)
    {
        _importer = definition.Module!.DefaultImporter;

        // get mscorlib
        var mscorlibReference = definition.Module!.AssemblyReferences.First(a => a.Name == "mscorlib");
        var mscorlib = mscorlibReference.Resolve()!.Modules[0];

        // get exception constructor
        var exception = mscorlib.TopLevelTypes.First(t => t.FullName == "System.Exception");
        _exceptionConstructor = exception.Methods.First(m =>
            m.Name == ".ctor" && m.Parameters is [{ ParameterType.FullName: "System.String" }]);
    }

    private void TranslateInstruction(MilInstruction instruction)
    {
        var instructions = _methodBody.Instructions;

        // now actually translate it
        switch (instruction.OpCode)
        {
            case MilOpCode.Unknown:
                if (instruction.Operands[0] != null)
                    AddException((string)instruction.Operands[0]!);

                break;
            case MilOpCode.Nop:
                break;

            case MilOpCode.Move:
                instructions.Add(CilOpCodes.Ldloc, _locals[(MilLocalVariable)instruction.Operands[1]!]);
                instructions.Add(CilOpCodes.Stloc, _locals[(MilLocalVariable)instruction.Operands[0]!]);
                break;
            case MilOpCode.ShiftStack:
                AddException("MilOpCode.ShiftStack should not exist at this point!");
                break;
            case MilOpCode.Phi:
                break;
            case MilOpCode.Call:
                var method = (MethodDefinition)instruction.Operands[1]!;
                if (!method.IsStatic)
                    instructions.Add(CilOpCodes.Ldarg_0);
                foreach (var parameter in instruction.Operands.Skip(2))
                    instructions.Add(CilOpCodes.Ldloc, _locals[(MilLocalVariable)parameter!]);
                instructions.Add(CilOpCodes.Call, _importer.ImportMethod(method));
                if (method.Signature!.ReturnsValue)
                    instructions.Add(CilOpCodes.Stloc, _locals[(MilLocalVariable)instruction.Operands[0]!]);
                break;
            case MilOpCode.Return:
                // if it's not null then there is return value
                if (instruction.Operands[0] is MilLocalVariable local)
                    instructions.Add(CilOpCodes.Ldloc, _locals[local]);
                instructions.Add(CilOpCodes.Ret);
                break;
            case MilOpCode.Jump:
                break;
            case MilOpCode.JumpTrue:
                break;
            case MilOpCode.JumpFalse:
                break;
            case MilOpCode.JumpEqual:
                break;
            case MilOpCode.JumpGreater:
                break;
            case MilOpCode.JumpLess:
                break;
            case MilOpCode.CheckEqual:
                break;
            case MilOpCode.CheckGreater:
                break;
            case MilOpCode.CheckLess:
                break;
            case MilOpCode.Add:
                break;
            case MilOpCode.Subtract:
                break;
            case MilOpCode.Multiply:
                break;
            case MilOpCode.Divide:
                break;
            case MilOpCode.And:
                break;
            case MilOpCode.Or:
                break;
            case MilOpCode.Xor:
                break;
            case MilOpCode.Not:
                break;
            case MilOpCode.Negate:
                break;
            case MilOpCode.ShiftRight:
                break;
            case MilOpCode.ShiftLeft:
                break;
            default:
                AddException($"Unknown opcode in MIL to CIL translator: {instruction.OpCode}");
                break;
        }
    }

    private void AddException(string message)
    {
        var instructions = _methodBody.Instructions;
        instructions.Add(CilOpCodes.Ldstr, message);
        instructions.Add(CilOpCodes.Newobj, _importer.ImportMethod(_exceptionConstructor));
        instructions.Add(CilOpCodes.Throw);
    }
}
