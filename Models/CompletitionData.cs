using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace industREAL.CAN.CanViewer.Models
{
    public class CompletionData : ICompletionData
    {
        public CompletionData(string text)
        {
            Text = text;
        }

        public string Text { get; }

        public object Content => Text;

        public object? Description => $"Insert `{Text}`";

        public IImage Image => null;

        public double Priority => 1;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var document = textArea.Document;
            int offset = completionSegment.Offset;

            // Find the start of the word under cursor
            int startOffset = offset;
            while (startOffset > 0 && char.IsLetterOrDigit(document.GetCharAt(startOffset - 1)))
            {
                startOffset--;
            }

            int length = offset - startOffset;

            // Replace only the partial segment (existing typed text)
            document.Replace(startOffset, length, Text);
        }
    }

}
