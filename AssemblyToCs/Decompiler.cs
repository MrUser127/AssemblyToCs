using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

namespace AssemblyToCs;

/// <summary>
/// The main decompiler.
/// </summary>
public class Decompiler
{
    /// <summary>
    /// Settings for IL -> C# decompiler.
    /// </summary>
    public DecompilerSettings Settings = new DecompilerSettings()
    {
        AggressiveInlining = true,
        AlwaysUseBraces = false
    };

    /// <summary>
    /// Decompiles a method to .NET's IL. There is no return value, this sets the body.
    /// </summary>
    /// <param name="method">What method?</param>
    public void Decompile(MethodDefinition method)
    {
        try
        {
            ReplaceBodyWithException(method, "Decompilation not implemented");
        }
        catch (Exception e)
        {
            ReplaceBodyWithException(method, e.ToString());
        }
    }

    /// <summary>
    /// Decompiles a method as string.
    /// </summary>
    /// <param name="method">What method?</param>
    /// <param name="assemblyDirectory">Where are deps for this assembly?</param>
    /// <returns>The method as string.</returns>
    public string DecompileAsString(MethodDefinition method, string assemblyDirectory)
    {
        Decompile(method);

        // i don't want to overwrite original assembly, just to be sure
        var assemblyPath = Path.Combine(assemblyDirectory, method.Module!.Name + "_tmp.dll");
        method.Module!.Write(assemblyPath);

        var decompiler = new CSharpDecompiler(assemblyPath, Settings);
        var name = new FullTypeName(method.DeclaringType!.FullName);
        var typeInfo = decompiler.TypeSystem.FindType(name).GetDefinition()!;
        var token = typeInfo.Methods.FirstOrDefault(m => m.Name == method.Name)!.MetadataToken;
        var code = decompiler.DecompileAsString(token);

        File.Delete(assemblyPath);
        return code;
    }

    private static void ReplaceBodyWithException(MethodDefinition method, string exceptionText)
    {
        var importer = method.Module!.DefaultImporter;

        var mscorlibReference = method.Module.AssemblyReferences.First(a => a.Name == "mscorlib");
        var mscorlib = mscorlibReference.Resolve()!.Modules[0];

        var exceptionType = mscorlib.TopLevelTypes.First(t => t.FullName == "System.Exception");
        var exceptionConstructor = exceptionType.Methods.First(m =>
            m.Name == ".ctor" && m.Parameters is [{ ParameterType.FullName: "System.String" }]);

        method.CilMethodBody = new CilMethodBody(method);
        var instructions = method.CilMethodBody.Instructions;

        instructions.Add(CilOpCodes.Ldstr, exceptionText);
        instructions.Add(CilOpCodes.Newobj, importer.ImportMethod(exceptionConstructor));
        instructions.Add(CilOpCodes.Throw);
    }
}
