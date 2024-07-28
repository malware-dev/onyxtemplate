// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class Document
    {
        TemplateTypeDescriptor _typeDescriptor;

        Document(DocumentHeader header, ImmutableArray<DocumentBlock> blocks)
        {
            Header = header;
            Blocks = blocks;
        }

        public ImmutableArray<DocumentBlock> Blocks { get; }
        
        public DocumentHeader Header { get; }
        
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
        ///     Converts an input string to a C# identifier.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="camelCase">Produce camelCase rather than PascalCase.</param>
        /// <returns></returns>
        public static string CSharpify(string input, bool camelCase = false)
        {
            if (string.IsNullOrEmpty(input))
                return "_";

            var buffer = new StringBuilder();
            var firstChar = input[0];

            if (camelCase)
                buffer.Append(char.IsDigit(firstChar) ? '_' : char.ToLower(firstChar));
            else
                buffer.Append(char.IsDigit(firstChar) ? '_' : char.ToUpper(firstChar));

            for (var i = 1; i < input.Length; i++)
            {
                var c = input[i];

                if (char.IsLetterOrDigit(c))
                    buffer.Append(c);
                else
                {
                    switch (c)
                    {
                        case 'æ':
                            buffer.Append("ae");
                            break;
                        case 'ø':
                            buffer.Append("oe");
                            break;
                        case 'å':
                            buffer.Append("aa");
                            break;
                        case 'ü':
                            buffer.Append("ue");
                            break;
                        case 'ö':
                            buffer.Append("oe");
                            break;
                        case 'ä':
                            buffer.Append("ae");
                            break;
                        case 'ß':
                            buffer.Append("ss");
                            break;
                        default:
                            buffer.Append('_');
                            break;
                    }
                }
            }

            return buffer.ToString();
        }

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
                var builder = descriptor.Up(simpleMacro.Field.Up);
                if (builder == null)
                    throw new DomException(simpleMacro.Field.Source, simpleMacro.Field.Name.Length, "Invalid field reference.");
                builder.WithField(simpleMacro.Field.Name.ToString(), TemplateFieldType.String);
            }

            var conditionalMacros = blocks.OfType<ConditionalMacro>();
            foreach (var conditional in conditionalMacros)
            {
                foreach (var section in conditional.IfSections)
                {
                    var builder = descriptor.Up(section.Field.Up);
                    if (builder == null)
                        throw new DomException(section.Field.Source, section.Field.Name.Length, "Invalid field reference.");
                    builder.WithField(section.Field.Name.ToString(), TemplateFieldType.Boolean);

                    ScanThisScope(descriptor, section.Blocks);
                }

                if (conditional.ElseSection != null)
                    ScanThisScope(descriptor, conditional.ElseSection.Blocks);
            }

            var loopMacros = blocks.OfType<ForEachMacroBlock>();
            foreach (var loop in loopMacros)
            {
                var builder = descriptor.Up(loop.Collection.Up);
                if (builder == null)
                    throw new DomException(loop.Collection.Source, loop.Collection.Name.Length, "Invalid field reference.");
                builder.WithField(loop.Collection.Name.ToString(),
                    TemplateFieldType.String,
                    p =>
                    {
                        p.AsCollection();
                        // If this loop only ever reference a single field which is the loop variable,
                        // this is a simple collection of strings.
                        if (loop.Blocks.All(b => b is TextBlock || b is SimpleMacroBlock sm && sm.Field.Up == 0 && sm.Field.Name.EqualsIgnoreCase(loop.Variable)))
                            return;

                        var complexTypeName = loop.Variable + "Item";
                        var complexType = new TemplateTypeDescriptor.Builder(descriptor)
                            .WithName(complexTypeName);
                        ScanThisScope(complexType, loop.Blocks);
                        builder.WithComplexType(complexType);
                        p.WithType(complexTypeName);
                    });
            }
        }

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

            StringSegment word;
            bool isMacroReference;
            switch (end.Current.Kind)
            {
                case TokenKind.Word:
                    isMacroReference = false;
                    word = end.Current.Image;
                    end++;
                    break;
                case TokenKind.First when up == 0:
                case TokenKind.Last when up == 0:
                case TokenKind.Middle when up == 0:
                case TokenKind.Odd when up == 0:
                case TokenKind.Even when up == 0:
                    isMacroReference = true;
                    word = end.Current.Image;
                    end++;
                    break;
                default:
                    reference = default;
                    return false;
            }

            ptr = end;
            reference = new DocumentFieldReference(word, up, isMacroReference);
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