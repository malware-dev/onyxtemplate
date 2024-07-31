// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    /// A macro that contains conditional sections.
    /// </summary>
    public class ConditionalMacro : DocumentBlock
    {
        /// <summary>
        /// Creates a new instance of <see cref="ConditionalMacro"/>.
        /// </summary>
        /// <param name="ifSections"></param>
        /// <param name="elseSection"></param>
        public ConditionalMacro(ImmutableArray<IfMacroSection> ifSections, ElseMacroSection elseSection)
        {
            IfSections = ifSections;
            ElseSection = elseSection;
        }

        /// <summary>
        /// A list of if sections, to be evaluated in order. They are mutually exclusive, so only one will be rendered.
        /// </summary>
        public ImmutableArray<IfMacroSection> IfSections { get; }
        
        /// <summary>
        /// An optional else section, to be rendered if none of the if sections are true.
        /// </summary>
        public ElseMacroSection ElseSection { get; }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override bool NeedsMacroState()
        {
            foreach (var section in IfSections)
            {
                if (section.NeedsMacroState())
                    return true;
            }

            if (ElseSection != null && ElseSection.NeedsMacroState())
                return true;

            return false;
        }
    }
}