using System;
using System.Collections.Generic;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     A simple static text block.
    /// </summary>
    public class TextBlock : DocumentBlock
    {
        /// <summary>
        ///     Creates a new instance of <see cref="TextBlock" />.
        /// </summary>
        /// <param name="text"></param>
        public TextBlock(StringSegment text)
        {
            Text = text;
        }

        /// <summary>
        ///     The text of the block.
        /// </summary>
        public StringSegment Text { get; }

        /// <inheritdoc />
        public override string ToString() => Text.ToString();

        /// <inheritdoc />
        public override IEnumerable<DocumentBlock> Descendants() => Array.Empty<DocumentBlock>();

        /// <inheritdoc />
        public override bool NeedsMacroState() => false;
    }
}