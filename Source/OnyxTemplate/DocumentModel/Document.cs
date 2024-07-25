// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class Document
    {
        Document(DocumentHeader header, ImmutableArray<DocumentBlock> blocks)
        {
            Header = header;
            Blocks = blocks;
        }

        public ImmutableArray<DocumentBlock> Blocks { get; }

        public DocumentHeader Header { get; }

        public static Document Parse(string source)
        {
            var blocks = ImmutableArray.CreateBuilder<DocumentBlock>();
            var ptr = new TextPtr(source);
            TryReadHeader(ref ptr, out var header);
            if (ReadBlocks(ref ptr, blocks) != null)
                throw new DomException(ptr, "Unexpected token.");
            return new Document(header, blocks.ToImmutable());
        }

        static IntermediateMacro ReadBlocks(ref TextPtr ptr, ImmutableArray<DocumentBlock>.Builder blocks)
        {
            var start = ptr;

            void flushTextBlock(in TextPtr here)
            {
                if (start.Index >= here.Index) return;
                blocks.Add(new TextBlock(start.TakeUntil(here).ToString()));
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
                throw new DomException(tptr.TextPtr, "Expected variable name.");
            var variable = tptr.Current.Image;
            tptr++;

            if (!tptr.IsKind(TokenKind.Word) || !tptr.Current.Image.EqualsIgnoreCase(Keywords.In))
                throw new DomException(tptr.TextPtr, "Expected 'in' keyword.");

            tptr++;
            if (!TryReadFieldReference(ref tptr, out var collection))
                throw new DomException(tptr.TextPtr, "Expected field reference.");

            var blocks = ImmutableArray.CreateBuilder<DocumentBlock>();
            macro = ReadBlocks(ref end, blocks);
            if (!macro.IsKind(TokenKind.Next))
                throw new DomException(end, "Expected \"next\" token.");

            loop = new ForEachMacroBlock(variable.ToString(), collection, blocks.ToImmutable());
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
                throw new DomException(tptr.TextPtr, "Expected field reference.");

            var conditionals = new List<ConditionalMacroSection>();
            var kind = macro.Kind;
            while (true)
            {
                var blocks = ImmutableArray.CreateBuilder<DocumentBlock>();
                macro = ReadBlocks(ref end, blocks);
                switch (macro.Kind)
                {
                    case TokenKind.ElseIf:
                        if (kind == TokenKind.Else)
                            throw new DomException(end, "Else block must be last in a conditional block.");
                        conditionals.Add(new IfMacroSection(isNot, field, blocks.ToImmutable()));

                        tptr = new TokenPtr(macro) + 1;
                        isNot = false;
                        if (tptr.IsKind(TokenKind.Word) && tptr.Current.Image.EqualsIgnoreCase(Keywords.Not))
                        {
                            isNot = true;
                            tptr++;
                        }

                        if (!TryReadFieldReference(ref tptr, out field))
                            throw new DomException(tptr.TextPtr, "Expected field reference.");
                        kind = TokenKind.ElseIf;
                        break;
                    case TokenKind.Else:
                        if (kind == TokenKind.Else)
                            throw new DomException(end, "Else block must be last in a conditional block.");
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
                                throw new DomException(end, "Unexpected token.");
                        }

                        conditional = new ConditionalMacro(ImmutableArray.CreateRange(conditionals), final);
                        ptr = end;
                        return true;
                    default:
                        throw new DomException(end, "Unexpected token.");
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

                ptr++;
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

            switch (end.Current.Kind)
            {
                case TokenKind.Word:
                case TokenKind.First when up == 0:
                case TokenKind.Last when up == 0:
                case TokenKind.Middle when up == 0:
                case TokenKind.Odd when up == 0:
                case TokenKind.Even when up == 0:
                    end++;
                    break;
                default:
                    reference = default;
                    return false;
            }

            var word = end.Current.Image;
            ptr = end;
            reference = new DocumentFieldReference(word.ToString(), up);
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
                throw new DomException(ptr, "Header is expected to be alone on a line.");

            var indent = false;
            var publicVisibility = false;

            for (var i = 1; i < macro.Tokens.Length; i++)
            {
                if (macro.Tokens[i].Kind != TokenKind.Word)
                    throw new DomException(ptr, "Unexpected token in header.");

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
            }

            header = new DocumentHeader(indent, publicVisibility);
        }

        // static bool TryReadCommand(ref TextPtr ptr, out StringSegment command)
        // {
        //     var end = ptr;
        //     if (!end.StartsWith("$"))
        //     {
        //         command = default;
        //         return false;
        //     }
        //
        //     end++;
        //     if (TryReadWord(ref end, out command))
        //     {
        //         ptr = end;
        //         return true;
        //     }
        //
        //     return true;
        // }
        //
        // static bool TryReadKeyword(ref TextPtr ptr, StringSegment keyword)
        // {
        //     var end = ptr;
        //     if (!TryReadWord(ref end, out var word))
        //         return false;
        //     if (!word.EqualsIgnoreCase(keyword))
        //         return false;
        //     ptr = end;
        //     return true;
        // }
        //
        // static bool TryReadWord(ref TextPtr ptr, out StringSegment word)
        // {
        //     var end = ptr;
        //     if (!end.IsStartOfWord())
        //     {
        //         word = default;
        //         return false;
        //     }
        //
        //     end++;
        //     while (end.IsWordCharacter())
        //         end++;
        //     word = ptr.TakeUntil(end);
        //     ptr = end;
        //     return true;
        // }

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