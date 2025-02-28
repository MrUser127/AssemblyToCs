using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using AssemblyToCs.Mil;

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
    /// Invoked before starting decompilation.
    /// </summary>
    public Action<Method> PreDecompile = (_) => { };

    /// <summary>
    /// Invoked after simplifying.
    /// </summary>
    public Action<Method> PostSimplify = (_) => { };

    /// <summary>
    /// Invoked after building the control flow graph.
    /// </summary>
    public Action<Method> PostBuildCfg = (_) => { };

    /// <summary>
    /// Invoked after simplifying the control flow graph.
    /// </summary>
    public Action<Method> PostSimplifyCfg = (_) => { };

    /// <summary>
    /// Invoked after building dominance info.
    /// </summary>
    public Action<Method> PostBuildDominance = (_) => { };

    /// <summary>
    /// Invoked after decompilation.
    /// </summary>
    public Action<Method> PostDecompile = (_) => { };

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
    /// <param name="source">Where did this log come from?</param>
    public void Info(string text, string source = "Decompiler") => InfoLog(text, source);

    /// <summary>
    /// Logs warning message.
    /// </summary>
    /// <param name="text">Text.</param>
    /// <param name="source">Where did this log come from?</param>
    public void Warn(string text, string source = "Decompiler") => WarnLog(text, source);

    /// <summary>
    /// Logs error message.
    /// </summary>
    /// <param name="text">Text.</param>
    /// <param name="source">Where did this log come from?</param>
    public void Error(string text, string source = "Decompiler") => ErrorLog(text, source);

    /// <summary>
    /// Decompiles a method to .NET's IL. There is no return value, this sets the body.
    /// </summary>
    /// <param name="method">What method?</param>
    public void Decompile(Method method)
    {
        var definition = method.Definition;
        Info($"Decompiling {definition.Name}...");

        try
        {
            PreDecompile(method);

            Info("Simplifying...");
            Simplifier.Simplify(method, this);
            PostSimplify(method);

            Info("Building CFG...");
            var cfg = new ControlFlowGraph();
            cfg.Build(method, this);
            method.FlowGraph = cfg;
            PostBuildCfg(method);

            Info("Simplifying control flow...");
            Simplifier.SimplifyControlFlow(method, this);
            PostSimplifyCfg(method);

            Info("Building dominance info...");
            var dominance = Dominance.Build(method);
            method.Dominance = dominance;
            PostBuildDominance(method);

            PostDecompile(method);

            ReplaceBodyWithException(definition, "Decompilation not implemented");
            Info("Done!");
        }
        catch (Exception e)
        {
            ReplaceBodyWithException(definition, "Decompilation failed: " + e);
            Error($"Decompilation failed: {e}");
        }
    }

    /// <summary>
    /// Decompiles a method as string. This writes a copy of original assembly with 1 method decompiled to assemblyDirectory and then deletes it.
    /// </summary>
    /// <param name="method">What method?</param>
    /// <param name="assemblyDirectory">Where are deps for this assembly?</param>
    /// <returns>The method as string.</returns>
    public string DecompileAsString(Method method, string assemblyDirectory)
    {
        var definition = method.Definition;
        Decompile(method);

        Info("Decompiling IL to C#...");

        var assemblyPath = Path.Combine(assemblyDirectory, definition.Module!.Name + "_tmp.dll");
        definition.Module!.Write(assemblyPath);

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
