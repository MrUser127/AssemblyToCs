using System.Text;
using Asm2Cs.IL;
using Asm2Cs.Transforms;

namespace Asm2Cs;

/// <summary>
/// The main decompiler class.
/// </summary>
public class Decompiler
{
    /// <summary>
    /// All transforms that will be applied to functions.
    /// </summary>
    public List<ITransform> Transforms =
    [
        new XorToMoveZeroTransform()
    ];

    public Action<string, string> InfoLog = (_, _) => { };

    public Action<string, string> WarnLog = (_, _) => { };

    public Action<string, string> ErrorLog = (_, _) => { };

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="source">Message source.</param>
    public void LogInfo(string message, string source = "Decompiler") => InfoLog(message, source);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="source">Message source.</param>
    public void LogWarn(string message, string source = "Decompiler") => WarnLog(message, source);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="source">Message source.</param>
    public void LogError(string message, string source = "Decompiler") => ErrorLog(message, source);

    /// <summary>
    /// Decompiles a function as string.
    /// </summary>
    /// <param name="function">The function.</param>
    public string DecompileFunctionAsString(Function function)
    {
        var controlFlowGraph = new ControlFlowGraph(function);

        foreach (var transform in Transforms)
            transform.Run(function, this);

        var sb = new StringBuilder();

        foreach (var comment in function.Comments.Where(comment => comment.Instruction == null))
            sb.AppendLine($"// {comment.Text}");

        sb.AppendLine(function.PrintSignature());
        sb.AppendLine("{");
        sb.AppendLine("    /*");
        sb.AppendLine("        decompilation not fully implemented");
        sb.AppendLine("        il instructions:");
        sb.AppendLine();
        sb.AppendLine($"        {string.Join(Environment.NewLine + "        ", function.Instructions)}");
        sb.AppendLine("    */");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
