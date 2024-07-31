using System;
using System.Collections.Generic;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    /// Renders the value of a single field. 
    /// </summary>
    public class SimpleMacroBlock : DocumentBlock
    {
        /// <summary>
        /// Creates a new instance of <see cref="SimpleMacroBlock"/>.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="indent"></param>
        public SimpleMacroBlock(DocumentFieldReference field, bool indent)
        {
            Field = field;
            Indent = indent;
        }

        /// <summary>
        /// A reference to the field to render.
        /// </summary>
        public DocumentFieldReference Field { get; }

        /// <summary>
        /// Whether the field should be indented.
        /// </summary>
        /// <remarks>
        /// If the header of the document specifies that the template should be indented, this value will be ignored and the
        /// field will be indented regardless.
        /// </remarks>
        public bool Indent { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{{ ");
            sb.Append(Field);
            if (Indent)
                sb.Append(" indent");
            sb.Append(" }}");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override IEnumerable<DocumentBlock> Descendants() => Array.Empty<DocumentBlock>();

        /// <inheritdoc />
        public override bool NeedsMacroState() => Field.MetaMacroKind != MetaMacroKind.None;
    }
}