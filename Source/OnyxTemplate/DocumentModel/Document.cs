// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     Represents a document used to generate template code.
    /// </summary>
    public class Document
    {

        TemplateTypeDescriptor _typeDescriptor;

        Document(DocumentHeader header, ImmutableArray<DocumentBlock> blocks)
        {
            Header = header;
            Blocks = blocks;
        }

        /// <summary>
        ///     All the blocks in this document.
        /// </summary>
        public ImmutableArray<DocumentBlock> Blocks { get; }

        /// <summary>
        ///     An optional header for the document, containing information on how the template should be generated.
        /// </summary>
        public DocumentHeader Header { get; }

        /// <summary>
        ///     Returns all descendants of this document.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DocumentBlock> Descendants()
        {
            foreach (var block in Blocks)
            {
                yield return block;
                foreach (var subBlock in block.Descendants())
                    yield return subBlock;
            }
        }

        /// <summary>
        ///     Determines if this document needs a macro state to be rendered (will also evaluate any descendants).
        /// </summary>
        /// <returns></returns>
        public bool NeedsMacroState() => Descendants().Any(b => b.NeedsMacroState());

        /// <summary>
        ///     Creates a template type descriptor from this document, containing all fields and types needed for a template
        ///     generated from this document.
        /// </summary>
        /// <returns></returns>
        public TemplateTypeDescriptor ToTemplateTypeDescriptor()
        {
            if (_typeDescriptor != null)
                return _typeDescriptor;

            var descriptor = new TemplateTypeDescriptor.Builder();

            ScanThisScope(descriptor, Blocks);

            _typeDescriptor = descriptor.Build();
            return _typeDescriptor;
        }

        static void ScanThisScope(TemplateTypeDescriptor.Builder descriptor, IReadOnlyList<DocumentBlock> blocks)
        {
            var mySimpleMacros = blocks.OfType<SimpleMacroBlock>();
            foreach (var simpleMacro in mySimpleMacros)
            {
                if (simpleMacro.Field.MetaMacroKind != MetaMacroKind.None)
                    continue;

                var builder = descriptor.Up(simpleMacro.Field.Up);
                if (builder == null)
                    throw new DomException(simpleMacro.Field.Source, simpleMacro.Field.Name.Length, "Invalid field reference.");
                builder.WithField(simpleMacro.Field.Name, TemplateFieldType.String);
            }

            var conditionalMacros = blocks.OfType<ConditionalMacro>();
            foreach (var conditional in conditionalMacros)
            {
                foreach (var section in conditional.IfSections)
                {
                    var builder = descriptor.Up(section.Field.Up);
                    if (builder == null)
                        throw new DomException(section.Field.Source, section.Field.Name.Length, "Invalid field reference.");
                    if (section.Field.MetaMacroKind == MetaMacroKind.None)
                        builder.WithField(section.Field.Name, TemplateFieldType.Boolean);

                    ScanThisScope(descriptor, section.Blocks);
                }

                if (conditional.ElseSection != null)
                    ScanThisScope(descriptor, conditional.ElseSection.Blocks);
            }

            var loopMacros = blocks.OfType<ForEachMacroBlock>();
            foreach (var loop in loopMacros)
            {
                if (loop.Collection.MetaMacroKind != MetaMacroKind.None)
                    throw new DomException(loop.Collection.Source, loop.Collection.Name.Length, "Cannot loop over a macro reference.");
                var builder = descriptor.Up(loop.Collection.Up);
                if (builder == null)
                    throw new DomException(loop.Collection.Source, loop.Collection.Name.Length, "Invalid field reference.");
                builder.WithField(loop.Collection.Name,
                    TemplateFieldType.String,
                    p =>
                    {
                        p.AsCollection();
                        // If this loop only ever reference a single field which is the loop variable,
                        // this is a simple collection of strings.

                        var n = AllFields(loop, 0).Distinct().Count(f => f.Up >= 1 && f.MetaMacroKind == MetaMacroKind.None);
                        if (n <= 1)
                            return;

                        var complexTypeName = Identifier.MakeSafe(loop.Collection.Name + "Item");
                        var complexType = new TemplateTypeDescriptor.Builder(descriptor)
                            .WithName(complexTypeName);
                        ScanThisScope(complexType, loop.Blocks);
                        builder.WithComplexType(complexType);
                        p.WithType(complexTypeName);
                    });
            }
        }

        static IEnumerable<DocumentFieldReference> AllFields(ConditionalMacroSection section, int scope)
        {
            switch (section)
            {
                case null:
                    yield break;
                case ElseMacroSection elseMacroSection:
                    foreach (var block in elseMacroSection.Blocks)
                    {
                        foreach (var field in AllFields(block, scope))
                            yield return field;
                    }

                    break;
                case IfMacroSection ifMacroSection:
                    yield return new DocumentFieldReference(ifMacroSection.Field.Name, scope - ifMacroSection.Field.Up, ifMacroSection.Field.MetaMacroKind);
                    foreach (var block in ifMacroSection.Blocks)
                    {
                        foreach (var field in AllFields(block, scope))
                            yield return field;
                    }

                    break;
            }
        }

        static IEnumerable<DocumentFieldReference> AllFields(DocumentBlock block, int scope)
        {
            switch (block)
            {
                case SimpleMacroBlock simpleMacro:
                    yield return new DocumentFieldReference(simpleMacro.Field.Name, scope - simpleMacro.Field.Up, simpleMacro.Field.MetaMacroKind);
                    break;
                case ConditionalMacro conditionalMacro:
                    foreach (var section in conditionalMacro.IfSections)
                    {
                        foreach (var field in AllFields(section, scope))
                            yield return field;
                    }

                    if (conditionalMacro.ElseSection != null)
                    {
                        foreach (var field in AllFields(conditionalMacro.ElseSection, scope))
                            yield return field;
                    }

                    break;
                case ForEachMacroBlock loopMacro:
                    yield return new DocumentFieldReference(loopMacro.Collection.Name, scope - loopMacro.Collection.Up, loopMacro.Collection.MetaMacroKind);
                    foreach (var subBlock in loopMacro.Blocks)
                    {
                        foreach (var field in AllFields(subBlock, scope + 1))
                            yield return field;
                    }

                    break;
            }
        }

        static int CountFields(ConditionalMacroSection section, int scope)
        {
            switch (section)
            {
                case null:
                    return 0;
                case ElseMacroSection elseMacroSection:
                    return elseMacroSection.Blocks.Sum(b => CountFields(b, scope));
                case IfMacroSection ifMacroSection:
                    return (ifMacroSection.Field.MetaMacroKind == MetaMacroKind.None ? 1 : 0) + ifMacroSection.Blocks.Sum(b => CountFields(b, scope));
                default:
                    throw new ArgumentOutOfRangeException(nameof(section));
            }
        }

        static int CountFields(DocumentBlock block, int scope)
        {
            switch (block)
            {
                case SimpleMacroBlock simpleMacro when scope - simpleMacro.Field.Up < 0:
                    return 0;
                case SimpleMacroBlock simpleMacro:
                    return simpleMacro.Field.MetaMacroKind == MetaMacroKind.None ? 1 : 0;
                case ConditionalMacro conditionalMacro:
                    return conditionalMacro.IfSections.Sum(b => CountFields(b, scope)) + CountFields(conditionalMacro.ElseSection, scope);
                case ForEachMacroBlock loopMacro:
                    return loopMacro.Blocks.Sum(b => CountFields(b, scope + 1));
                default:
                    return 0;
            }
        }

        /// <summary>
        ///     Parses a document from a string.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="DomException"></exception>
        public static Document Parse(string source)
        {
            var blocks = ImmutableArray.CreateBuilder<DocumentBlock>();
            var ptr = new TextPtr(source);
            TryReadHeader(ref ptr, out var header);
            if (ReadBlocks(ref ptr, blocks) != null)
                throw new DomException(ptr, 1, "Unexpected token.");
            return new Document(header, blocks.ToImmutable());
        }

        static IntermediateMacro ReadBlocks(ref TextPtr ptr, ImmutableArray<DocumentBlock>.Builder blocks)
        {
            var start = ptr;

            void flushTextBlock(in TextPtr here)
            {
                if (start.Index >= here.Index) return;
                blocks.Add(new TextBlock(start.TakeUntil(here)));
                start = here;
            }

            while (!ptr.IsPastEnd())
            {
                var end = ptr;
                if (IntermediateMacro.TryParse(ref ptr, out var macro))
                {
                    switch (macro[0].Kind)
                    {
                        case TokenKind.End:
                        case TokenKind.Next:
                        case TokenKind.Else:
                        case TokenKind.ElseIf:
                            flushTextBlock(end);
                            return macro;
                    }

                    if (TryReadSimpleMacro(macro, out var simpleMacro))
                    {
                        flushTextBlock(end);
                        start = ptr;
                        blocks.Add(simpleMacro);
                        continue;
                    }

                    if (TryReadConditional(macro, ref ptr, out var conditional))
                    {
                        flushTextBlock(end);
                        start = ptr;
                        blocks.Add(conditional);
                        continue;
                    }

                    if (TryReadForEach(macro, ref ptr, out var loop))
                    {
                        flushTextBlock(end);
                        start = ptr;
                        blocks.Add(loop);
                        continue;
                    }
                }

                ptr++;
            }

            flushTextBlock(ptr);
            return null;
        }

        static bool TryReadForEach(IntermediateMacro macro, ref TextPtr ptr, out DocumentBlock loop)
        {
            var end = ptr;
            if (!macro.IsKind(TokenKind.ForEach))
            {
                loop = default;
                return false;
            }

            var tptr = new TokenPtr(macro) + 1;
            if (!tptr.IsKind(TokenKind.Word))
                throw new DomException(tptr.TextPtr, tptr.Current.Image.Length, "Expected variable name.");
            var variable = tptr.Current.Image;
            tptr++;

            if (!tptr.IsKind(TokenKind.Word) || !tptr.Current.Image.EqualsIgnoreCase(Keywords.In))
                throw new DomException(tptr.TextPtr, tptr.Current.Image.Length, "Expected 'in' keyword.");

            tptr++;
            if (!TryReadFieldReference(ref tptr, out var collection))
                throw new DomException(tptr.TextPtr, tptr.Current.Image.Length, "Expected field reference.");

            var blocks = ImmutableArray.CreateBuilder<DocumentBlock>();
            macro = ReadBlocks(ref end, blocks);
            if (!macro.IsKind(TokenKind.Next))
                throw new DomException(end, 1, "Expected \"next\" token.");

            loop = new ForEachMacroBlock(variable, collection, blocks.ToImmutable());
            ptr = end;
            return true;
        }

        static bool TryReadConditional(IntermediateMacro macro, ref TextPtr ptr, out DocumentBlock conditional)
        {
            var end = ptr;
            if (!macro.IsKind(TokenKind.If))
            {
                conditional = default;
                return false;
            }

            var tptr = new TokenPtr(macro) + 1;
            var isNot = false;
            if (tptr.IsKind(TokenKind.Word) && tptr.Current.Image.EqualsIgnoreCase(Keywords.Not))
            {
                isNot = true;
                tptr++;
            }

            if (!TryReadFieldReference(ref tptr, out var field))
                throw new DomException(tptr.TextPtr, tptr.Current.Image.Length, "Expected field reference.");

            var conditionals = new List<IfMacroSection>();
            var kind = macro.Kind;
            while (true)
            {
                var blocks = ImmutableArray.CreateBuilder<DocumentBlock>();
                macro = ReadBlocks(ref end, blocks);
                switch (macro.Kind)
                {
                    case TokenKind.ElseIf:
                        if (kind == TokenKind.Else)
                            throw new DomException(end, 1, "Else block must be last in a conditional block.");
                        conditionals.Add(new IfMacroSection(isNot, field, blocks.ToImmutable()));

                        tptr = new TokenPtr(macro) + 1;
                        isNot = false;
                        if (tptr.IsKind(TokenKind.Word) && tptr.Current.Image.EqualsIgnoreCase(Keywords.Not))
                        {
                            isNot = true;
                            tptr++;
                        }

                        if (!TryReadFieldReference(ref tptr, out field))
                            throw new DomException(tptr.TextPtr, tptr.Current.Image.Length, "Expected field reference.");
                        kind = TokenKind.ElseIf;
                        break;
                    case TokenKind.Else:
                        if (kind == TokenKind.Else)
                            throw new DomException(end, 1, "Else block must be last in a conditional block.");
                        conditionals.Add(new IfMacroSection(isNot, field, blocks.ToImmutable()));
                        kind = TokenKind.Else;
                        break;
                    case TokenKind.End:
                        ElseMacroSection final = null;
                        switch (kind)
                        {
                            case TokenKind.Else:
                                final = new ElseMacroSection(blocks.ToImmutable());
                                break;
                            case TokenKind.ElseIf:
                            case TokenKind.If:
                                conditionals.Add(new IfMacroSection(isNot, field, blocks.ToImmutable()));
                                break;
                            default:
                                throw new DomException(end, 1, "Unexpected token.");
                        }

                        conditional = new ConditionalMacro(ImmutableArray.CreateRange(conditionals), final);
                        ptr = end;
                        return true;
                    default:
                        throw new DomException(end, 1, "Unexpected token.");
                }
            }
        }

        static bool TryReadSimpleMacro(IntermediateMacro macro, out DocumentBlock simpleMacro)
        {
            var ptr = new TokenPtr(macro);
            if (!TryReadFieldReference(ref ptr, out var field))
            {
                simpleMacro = default;
                return false;
            }

            var indent = false;
            if (ptr.IsKind(TokenKind.Colon))
            {
                ptr++;
                if (!ptr.IsKind(TokenKind.Word))
                {
                    simpleMacro = default;
                    return false;
                }

                var flag = ptr.Current.Image;
                if (flag.EqualsIgnoreCase(Keywords.Indented) || flag.EqualsIgnoreCase(Keywords.Indent))
                    indent = true;
            }

            simpleMacro = new SimpleMacroBlock(field, indent);
            return true;
        }

        static bool TryReadFieldReference(ref TokenPtr ptr, out DocumentFieldReference reference)
        {
            var end = ptr;
            var up = 0;
            while (end.IsKind(TokenKind.Dot))
            {
                up++;
                end++;
            }

            var metaMacroKind = MetaMacroKind.None;
            StringSegment word;
            switch (end.Current.Kind)
            {
                case TokenKind.Word:
                    word = end.Current.Image;
                    end++;
                    break;
                case TokenKind.First:
                    metaMacroKind = MetaMacroKind.First;
                    word = new StringSegment("First");
                    end++;
                    break;
                case TokenKind.Last:
                    metaMacroKind = MetaMacroKind.Last;
                    word = new StringSegment("Last");
                    end++;
                    break;
                case TokenKind.Middle:
                    metaMacroKind = MetaMacroKind.Middle;
                    word = new StringSegment("Middle");
                    end++;
                    break;
                case TokenKind.Odd:
                    metaMacroKind = MetaMacroKind.Odd;
                    word = new StringSegment("Odd");
                    end++;
                    break;
                case TokenKind.Even:
                    metaMacroKind = MetaMacroKind.Even;
                    word = new StringSegment("Even");
                    end++;
                    break;
                default:
                    reference = default;
                    return false;
            }

            ptr = end;
            reference = new DocumentFieldReference(word, up, metaMacroKind);
            return true;
        }

        static void TryReadHeader(ref TextPtr ptr, out DocumentHeader header)
        {
            if (!IntermediateMacro.TryParse(ref ptr, TokenKind.Template, out var macro))
            {
                header = default;
                return;
            }

            if (!macro.IsLineMacro)
                throw new DomException(ptr, 1, "Header is expected to be alone on a line.");

            var indent = false;
            var publicVisibility = false;
            string description = null;

            for (var i = 1; i < macro.Tokens.Length; i++)
            {
                switch (macro.Tokens[i].Kind)
                {
                    case TokenKind.String:
                        if (description != null)
                            throw new DomException(ptr, 1, "Unexpected token in header.");
                        description = macro.Tokens[i].Image.ToString();
                        break;
                    case TokenKind.Word:
                        switch (macro.Tokens[i].Select(Keywords.Indent, Keywords.Indented, Keywords.Public))
                        {
                            case 0:
                            case 1:
                                indent = true;
                                break;
                            case 2:
                                publicVisibility = true;
                                break;
                        }

                        break;
                    default:
                        throw new DomException(ptr, 1, "Unexpected token in header.");
                }
            }

            header = new DocumentHeader(indent, publicVisibility, description);
        }

        /// <summary>
        ///     This static class contains all the meta macros available in the template language.
        /// </summary>
        /// <remarks>
        ///     Meta macros are special macros that are not fields, but rather provide information about the current context.
        /// </remarks>
        public static class MetaMacros
        {
            public static readonly TemplateFieldDescriptor First = new TemplateFieldDescriptor(new Identifier("First"), TemplateFieldType.Boolean, TemplateFieldType.None, new Identifier("$first"), null);
            public static readonly TemplateFieldDescriptor Last = new TemplateFieldDescriptor(new Identifier("Last"), TemplateFieldType.Boolean, TemplateFieldType.None, new Identifier("$last"), null);
            public static readonly TemplateFieldDescriptor Middle = new TemplateFieldDescriptor(new Identifier("Middle"), TemplateFieldType.Boolean, TemplateFieldType.None, new Identifier("$middle"), null);
            public static readonly TemplateFieldDescriptor Odd = new TemplateFieldDescriptor(new Identifier("Odd"), TemplateFieldType.Boolean, TemplateFieldType.None, new Identifier("$odd"), null);
            public static readonly TemplateFieldDescriptor Even = new TemplateFieldDescriptor(new Identifier("Even"), TemplateFieldType.Boolean, TemplateFieldType.None, new Identifier("$even"), null);
        }

        static class Keywords
        {
            public static readonly StringSegment Indent = new StringSegment("indent");
            public static readonly StringSegment Indented = new StringSegment("indented");
            public static readonly StringSegment Public = new StringSegment("public");
            public static readonly StringSegment Not = new StringSegment("not");
            public static readonly StringSegment In = new StringSegment("in");
        }
    }
}