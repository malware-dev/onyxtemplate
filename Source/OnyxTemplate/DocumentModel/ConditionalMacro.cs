// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.Collections.Immutable;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class ConditionalMacro : DocumentBlock
    {
        public ConditionalMacro(ImmutableArray<ConditionalMacroSection> ifSections, ElseMacroSection elseSection)
        {
            IfSections = ifSections;
            ElseSection = elseSection;
        }

        public ImmutableArray<ConditionalMacroSection> IfSections { get; }
        public ElseMacroSection ElseSection { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var section in IfSections)
                sb.Append(section);
            if (ElseSection != null)
                sb.Append(ElseSection);
            sb.Append("{{ $end }}");
            return sb.ToString();
        }
    }
}