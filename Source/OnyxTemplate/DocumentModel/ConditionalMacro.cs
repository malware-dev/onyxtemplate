// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class ConditionalMacro : DocumentBlock
    {
        public ConditionalMacro(ImmutableArray<IfMacroSection> ifSections, ElseMacroSection elseSection)
        {
            IfSections = ifSections;
            ElseSection = elseSection;
        }

        public ImmutableArray<IfMacroSection> IfSections { get; }
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

        public override IEnumerable<DocumentBlock> Descendants()
        {
            foreach (var section in IfSections)
            {
                foreach (var block in section.Descendants())
                    yield return block;
            }

            if (ElseSection != null)
            {
                foreach (var block in ElseSection.Descendants())
                    yield return block;
            }
        }
    }
}