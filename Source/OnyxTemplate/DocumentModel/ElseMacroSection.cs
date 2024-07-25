using System.Collections.Immutable;
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
    }
}