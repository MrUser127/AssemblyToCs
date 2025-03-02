using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using AssemblyToCs.Transforms;

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
    /// Transforms applied to methods.
    /// </summary>
    public List<ITransform> Transforms = new List<ITransform>()
    {
        new XorToMove(),
        new BuildCfg(),
        new RemoveUnreachableBlocks(),
        new AnalyzeStack(),
        new RemoveNops(), // stack analysis replaces shift stack instructions with nops so this needs to be here
        new MergeCallBlocks(), // initially blocks are split by calls for stack analysis
        //new RemoveRedundantAssignments(), // sometimes this works but it breaks often
        new BuildDominance(),
        new BuildSsa() // this must be after build dominance because this uses that info
    };

    /// <summary>
    /// Info log event.
    /// </summary>
    public Action<string, string> InfoLog = (_, _) => { };

    /// <summary>
    /// Warning log event.
    /// </summary>
    public Action<string, string> WarnLog = (_, _) => { };

    /// <summary>
    /// Error log event.
    /// </summary>
    public Action<string, string> ErrorLog = (_, _) => { };

    /// <summary>
    /// Logs info message.
    /// </summary>
    /// <param name="text">Text.</param>
    /// <param name="source">Message source.</param>
    public void Info(string text, string source = "Decompiler") => InfoLog(text, source);

    /// <summary>
    /// Logs warning message.
    /// </summary>
    /// <param name="text">Text.</param>
    /// <param name="source">Message source.</param>
    public void Warn(string text, string source = "Decompiler") => WarnLog(text, source);

    /// <summary>
    /// Logs error message.
    /// </summary>
    /// <param name="text">Text.</param>
    /// <param name="source">Message source.</param>
    public void Error(string text, string source = "Decompiler") => ErrorLog(text, source);

    /// <summary>
    /// Decompiles a method to .NET's IL. There is no return value, this sets the body.
    /// </summary>
    /// <param name="method">The method.</param>
    public void Decompile(Method method)
    {
        var definition = method.Definition;
        Info($"Decompiling {definition.Name}...");

        try
        {
            Info("Applying transforms...");
            foreach (var transform in Transforms)
                transform.Apply(method, this);

            ReplaceBodyWithException(definition, "Decompilation not implemented");
            Info("Low level analysis done!");
        }
        catch (Exception e)
        {
            ReplaceBodyWithException(definition, "Decompilation failed: " + e);
            Error($"Decompilation failed: {e}");
        }
    }

    /// <summary>
    /// Decompiles a method as string. This writes a copy of original assembly with 1 method decompiled to <paramref name="assemblyDirectory"/> and then deletes it.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="assemblyDirectory">The directory that has deps for the method.</param>
    /// <returns>The method as string.</returns>
    public string DecompileAsString(Method method, string assemblyDirectory)
    {
        var definition = method.Definition;
        Decompile(method);

        Info("Decompiling IL to C#...");

        // write it so that ilspy can resolve deps
        var assemblyPath = Path.Combine(assemblyDirectory, definition.Module!.Name + "_tmp.dll");
        definition.Module!.Write(assemblyPath);

        // decompile IL -> C#
        var decompiler = new CSharpDecompiler(assemblyPath, Settings);
        var name = new FullTypeName(definition.DeclaringType!.FullName);
        var typeInfo = decompiler.TypeSystem.FindType(name).GetDefinition()!;
        var token = typeInfo.Methods.FirstOrDefault(m => m.Name == definition.Name)!.MetadataToken;
        var code = decompiler.DecompileAsString(token);

        File.Delete(assemblyPath);
        Info("Done!");
        return code;
    }

    private static void ReplaceBodyWithException(MethodDefinition method, string exceptionText)
    {
        var importer = method.Module!.DefaultImporter;

        // get mscorlib
        var mscorlibReference = method.Module.AssemblyReferences.First(a => a.Name == "mscorlib");
        var mscorlib = mscorlibReference.Resolve()!.Modules[0];

        // get exception constructor
        var exception = mscorlib.TopLevelTypes.First(t => t.FullName == "System.Exception");
        var exceptionConstructor = exception.Methods.First(m =>
            m.Name == ".ctor" && m.Parameters is [{ ParameterType.FullName: "System.String" }]);

        // add instructions
        method.CilMethodBody = new CilMethodBody(method);
        var instructions = method.CilMethodBody.Instructions;

        instructions.Add(CilOpCodes.Ldstr, exceptionText);
        instructions.Add(CilOpCodes.Newobj, importer.ImportMethod(exceptionConstructor));
        instructions.Add(CilOpCodes.Throw);
    }
}
