// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class ForEachMacroBlock : DocumentBlock
    {
        public ForEachMacroBlock(StringSegment variable, DocumentFieldReference collection, ImmutableArray<DocumentBlock> blocks)
        {
            Variable = variable;
            Collection = collection;
            Blocks = blocks;
        }

        public StringSegment Variable { get; }
        public DocumentFieldReference Collection { get; }
        public ImmutableArray<DocumentBlock> Blocks { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{{ $foreach ");
            sb.Append(Variable);
            sb.Append(" in ");
            sb.Append(Collection);
            sb.Append(" }}");
            foreach (var block in Blocks)
                sb.Append(block);
            sb.Append("{{ $next }}");
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