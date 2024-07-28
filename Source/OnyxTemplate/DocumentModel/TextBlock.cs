using System;
using System.Collections.Generic;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class TextBlock : DocumentBlock
    {
        public TextBlock(StringSegment text)
        {
            Text = text;
        }

        public StringSegment Text { get; }

        public override string ToString() => Text.ToString();
        public override IEnumerable<DocumentBlock> Descendants() => Array.Empty<DocumentBlock>();
    }
}