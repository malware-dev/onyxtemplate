using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class ElseMacroSection : ConditionalMacroSection
    {
        public ElseMacroSection(ImmutableArray<DocumentBlock> blocks)
        {
            Blocks = blocks;
        }

        public ImmutableArray<DocumentBlock> Blocks { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{{ $else }}");
            foreach (var block in Blocks)
                sb.Append(block);
            return sb.ToString();
        }

        public override IEnumerable<DocumentBlock> Descendants()
        {
            foreach (var block in Blocks)
            {
                yield return block;
                foreach (var subBlock in block.Descendants())
                    yield return subBlock;
            }
        }

        public override bool NeedsMacroState() => Descendants().Any(b => b.NeedsMacroState());
    }
}