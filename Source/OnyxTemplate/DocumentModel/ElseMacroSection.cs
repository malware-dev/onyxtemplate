using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     A conditional section to be evaluated if any of the associated <see cref="IfMacroSection" />s are evaluated as
    ///     false.
    /// </summary>
    public class ElseMacroSection : ConditionalMacroSection
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ElseMacroSection" />.
        /// </summary>
        /// <param name="blocks"></param>
        public ElseMacroSection(ImmutableArray<DocumentBlock> blocks)
        {
            Blocks = blocks;
        }

        /// <summary>
        ///     All blocks in this section.
        /// </summary>
        public ImmutableArray<DocumentBlock> Blocks { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{{ $else }}");
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
        public override bool NeedsMacroState() => Descendants().Any(b => b.NeedsMacroState());
    }
}