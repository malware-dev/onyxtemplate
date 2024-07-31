using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     A conditional section to be evaluated if the associated field is true.
    /// </summary>
    public class IfMacroSection : ConditionalMacroSection
    {
        /// <summary>
        ///     Creates a new instance of <see cref="IfMacroSection" />.
        /// </summary>
        /// <param name="not"></param>
        /// <param name="field"></param>
        /// <param name="blocks"></param>
        public IfMacroSection(bool not, DocumentFieldReference field, ImmutableArray<DocumentBlock> blocks)
        {
            Blocks = blocks;
            Not = not;
            Field = field;
        }

        /// <summary>
        ///     Whether the field should be negated.
        /// </summary>
        public bool Not { get; }

        /// <summary>
        ///     A reference to the field to evaluate.
        /// </summary>
        public DocumentFieldReference Field { get; }

        /// <summary>
        ///     The blocks to render if the field is true.
        /// </summary>
        public ImmutableArray<DocumentBlock> Blocks { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{{ $if ");
            if (Not)
                sb.Append("not ");
            sb.Append(Field);
            sb.Append(" }}");
            foreach (var block in Blocks)
                sb.Append(block);
            return sb.ToString();
        }

        /// <inheritdoc />
        public override IEnumerable<DocumentBlock> Descendants()
        {
            foreach (var block in Blocks)
            {
                yield return block;
                foreach (var subBlock in block.Descendants())
                    yield return subBlock;
            }
        }

        /// <inheritdoc />
        public override bool NeedsMacroState()
        {
            if (Field.MetaMacroKind != MetaMacroKind.None)
                return true;

            return Descendants().Any(b => b.NeedsMacroState());
        }
    }
}