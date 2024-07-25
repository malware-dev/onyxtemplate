using System.Collections.Immutable;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class IfMacroSection : ConditionalMacroSection
    {
        public IfMacroSection(bool not, DocumentFieldReference field, ImmutableArray<DocumentBlock> blocks)
        {
            Blocks = blocks;
            Not = not;
            Field = field;
        }

        public bool Not { get; }
        public DocumentFieldReference Field { get; }
        public ImmutableArray<DocumentBlock> Blocks { get; }

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
    }
}