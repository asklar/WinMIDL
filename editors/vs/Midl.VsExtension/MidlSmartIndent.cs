using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Midl.VsExtension
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType("midl")]
    public class MidlSmartIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new MidlSmartIndent(textView);
        }
    }

    public class MidlSmartIndent : ISmartIndent
    {
        private readonly ITextView _textView;
        private const int IndentSize = 4;

        public MidlSmartIndent(ITextView textView)
        {
            _textView = textView;
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line)
        {
            if (line.LineNumber == 0)
                return 0;

            var snapshot = line.Snapshot;
            var prevLine = snapshot.GetLineFromLineNumber(line.LineNumber - 1);
            var prevText = prevLine.GetText();
            var currentText = line.GetText();

            int prevSpaces = CountLeadingSpaces(prevText);
            int desired = prevSpaces;

            // Increase indent after {
            if (prevText.TrimEnd().EndsWith("{"))
                desired += IndentSize;

            // Decrease indent for lines starting with }
            if (currentText.TrimStart().StartsWith("}"))
                desired = Math.Max(0, desired - IndentSize);

            return desired;
        }

        private static int CountLeadingSpaces(string text)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (c == ' ') count++;
                else if (c == '\t') count += 4;
                else break;
            }
            return count;
        }

        public void Dispose() { }
    }
}
