using System.Diagnostics;

namespace Midl.Analysis;

/// <summary>
/// Records timing data for each phase of the compilation pipeline.
/// </summary>
public sealed class CompilationProfile
{
    public TimeSpan Preprocess { get; set; }
    public TimeSpan Lex { get; set; }
    public TimeSpan Parse { get; set; }
    public TimeSpan SyntaxErrors { get; set; }
    public TimeSpan ImportResolve { get; set; }
    public TimeSpan Bind { get; set; }
    public TimeSpan Synthesize { get; set; }
    public TimeSpan VersionPropagate { get; set; }
    public TimeSpan Validate { get; set; }
    public TimeSpan Total { get; set; }

    public void Print(TextWriter writer)
    {
        var totalMs = Total.TotalMilliseconds;
        if (totalMs <= 0) totalMs = 0.001;

        writer.WriteLine($"[PROFILE] Total:           {FormatMs(Total),8} ");
        writer.WriteLine($"[PROFILE] Preprocess:      {FormatMs(Preprocess),8} ({Pct(Preprocess, totalMs),3}%)");
        writer.WriteLine($"[PROFILE] Lex:             {FormatMs(Lex),8} ({Pct(Lex, totalMs),3}%)");
        writer.WriteLine($"[PROFILE] Parse:           {FormatMs(Parse),8} ({Pct(Parse, totalMs),3}%)");
        writer.WriteLine($"[PROFILE] SyntaxErrors:    {FormatMs(SyntaxErrors),8} ({Pct(SyntaxErrors, totalMs),3}%)");
        writer.WriteLine($"[PROFILE] ImportResolve:   {FormatMs(ImportResolve),8} ({Pct(ImportResolve, totalMs),3}%)");
        writer.WriteLine($"[PROFILE] Bind:            {FormatMs(Bind),8} ({Pct(Bind, totalMs),3}%)");
        writer.WriteLine($"[PROFILE] Synthesize:      {FormatMs(Synthesize),8} ({Pct(Synthesize, totalMs),3}%)");
        writer.WriteLine($"[PROFILE] VersionPropagate:{FormatMs(VersionPropagate),8} ({Pct(VersionPropagate, totalMs),3}%)");
        writer.WriteLine($"[PROFILE] Validate:        {FormatMs(Validate),8} ({Pct(Validate, totalMs),3}%)");
    }

    private static string FormatMs(TimeSpan ts)
    {
        var ms = ts.TotalMilliseconds;
        return ms < 1 ? $"{ms:F3}ms" : $"{ms:F1}ms";
    }

    private static int Pct(TimeSpan phase, double totalMs)
        => (int)Math.Round(phase.TotalMilliseconds / totalMs * 100);
}
