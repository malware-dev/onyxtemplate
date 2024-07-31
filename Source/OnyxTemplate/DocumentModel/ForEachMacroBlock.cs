// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     A block that iterates over a collection and renders the inner blocks for each item in the collection.
    /// </summary>
    public class ForEachMacroBlock : DocumentBlock
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ForEachMacroBlock" />.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="collection"></param>
        /// <param name="blocks"></param>
        public ForEachMacroBlock(StringSegment variable, DocumentFieldReference collection, ImmutableArray<DocumentBlock> blocks)
        {
            Variable = variable;
            Collection = collection;
            Blocks = blocks;
        }

        /// <summary>
        ///     A variable to store the current item in the collection.
        /// </summary>
        public StringSegment Variable { get; }

        /// <summary>
        ///     Reference to the collection to iterate over.
        /// </summary>
        public DocumentFieldReference Collection { get; }

        /// <summary>
        ///     All blocks to render for each item in the collection.
        /// </summary>
        public ImmutableArray<DocumentBlock> Blocks { get; }

        /// <inheritdoc />
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