using System.Text;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Header;

public partial class HeaderEmitter
{
    /// <summary>
    /// Emits a RuntimeClass metadata comment block and RUNTIMECLASS_DEFINED guard
    /// around the activation string, matching real midl.exe output format.
    /// </summary>
    private static void EmitRuntimeClassWithComment(StringBuilder sb, RuntimeClassSymbol rc)
    {
        var fullName = rc.FullName;
        var activationName = fullName.Replace(".", "_");

        // Comment block
        sb.AppendLine("/*");
        sb.AppendLine(" *");
        sb.AppendLine($" * Class {fullName}");
        sb.AppendLine(" *");

        // List implemented interfaces
        if (rc.DefaultInterface != null || rc.ImplementedInterfaces.Count > 0)
        {
            sb.AppendLine(" * Class implements the following interfaces:");

            if (rc.DefaultInterface != null)
            {
                sb.AppendLine($" *    {rc.DefaultInterface.FullName} ** Default Interface **");
            }

            foreach (var iface in rc.ImplementedInterfaces)
            {
                if (iface != rc.DefaultInterface)
                {
                    sb.AppendLine($" *    {iface.FullName}");
                }
            }

            sb.AppendLine(" *");
        }

        // Threading model
        var threading = rc.ThreadingModel ?? "Both";
        var threadingDesc = threading switch
        {
            "Both" => "Both Single and Multi Threaded Apartment",
            "STA" => "Single Threaded Apartment",
            "MTA" => "Multi Threaded Apartment",
            _ => threading
        };
        sb.AppendLine($" * Class Threading Model:  {threadingDesc}");
        sb.AppendLine(" *");

        // Marshaling behavior
        var marshaling = rc.MarshalingBehavior ?? "Agile";
        var marshalingDesc = marshaling switch
        {
            "Agile" => "Agile - Class is agile",
            "Standard" => "Standard - Class does not support agile marshaling",
            "None" => "None - Class does not support marshaling",
            _ => marshaling
        };
        sb.AppendLine($" * Class Marshaling Behavior:  {marshalingDesc}");
        sb.AppendLine(" *");
        sb.AppendLine(" */");

        // RUNTIMECLASS_DEFINED guard
        var guardName = $"RUNTIMECLASS_{activationName}_DEFINED";
        sb.AppendLine($"#ifndef {guardName}");
        sb.AppendLine($"#define {guardName}");

        // Activation string
        sb.AppendLine($"extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_{activationName}[] = L\"{fullName}\";");

        sb.AppendLine($"#endif");
        sb.AppendLine();
    }
}
