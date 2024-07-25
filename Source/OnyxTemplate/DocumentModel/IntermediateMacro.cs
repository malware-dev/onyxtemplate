// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.Collections.Immutable;

namespace Mal.OnyxTemplate.DocumentModel
{
    readonly struct TokenPtr
    {
        readonly IntermediateMacro _macro;
        public readonly int Index;

        public TokenPtr(IntermediateMacro macro, int index = 0)
        {
            _macro = macro;
            Index = index;
        }
        
        public Token Current
        {
            get
            {
                if (Index < 0 || Index >= _macro.Tokens.Length)
                    return default;
                return _macro[Index];
            }
        }
        
        public TextPtr TextPtr => new TextPtr(Current.Image.Text, Current.Image.Start);

        public bool IsAtEnd => Index >= _macro.Tokens.Length;
        
        public static TokenPtr operator ++(TokenPtr ptr) => new TokenPtr(ptr._macro, ptr.Index + 1);
        public static TokenPtr operator --(TokenPtr ptr) => new TokenPtr(ptr._macro, ptr.Index - 1);
        public static TokenPtr operator +(TokenPtr ptr, int offset) => new TokenPtr(ptr._macro, ptr.Index + offset);
        public static TokenPtr operator -(TokenPtr ptr, int offset) => new TokenPtr(ptr._macro, ptr.Index - offset);

        public bool IsKind(TokenKind kind) => !IsAtEnd && Current.Kind == kind;
    }
    
    class IntermediateMacro
    {
        IntermediateMacro(ImmutableArray<Token> tokens, bool isLineMacro)
        {
            Tokens = tokens;
            IsLineMacro = isLineMacro;
        }

        public ImmutableArray<Token> Tokens { get; }
        public bool IsLineMacro { get; }
        public TokenKind Kind => Tokens.Length >= 1 ? Tokens[0].Kind : TokenKind.None;

        public Token this[int index]
        {
            get
            {
                if (index < 0 || index >= Tokens.Length)
                    return default;
                return Tokens[index];
            }
        }

        public static bool TryParse(ref TextPtr ptr, out IntermediateMacro macro)
        {
            var tokens = ImmutableArray.CreateBuilder<Token>();
            var end = ptr;
            var isLineMacro = end.IsAtStartOfLine();
            var hasTrivia = false;
            if (isLineMacro && end.IsWhitespace(true))
            {
                end = end.SkipWhitespace(true);
                hasTrivia = true;
            }

            if (!end.StartsWith("{{"))
            {
                macro = default;
                return false;
            }

            end = end.Skip(2);
            end = end.SkipWhitespace(true);
            while (!end.IsPastEnd() && !end.StartsWith("}}"))
            {
                if (end.StartsWithWord("$template", true))
                {
                    var image = end.Take(9);
                    end = end.Skip(9);
                    tokens.Add(new Token(TokenKind.Template, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.StartsWithWord("$if", true))
                {
                    var image = end.Take(3);
                    end = end.Skip(3);
                    tokens.Add(new Token(TokenKind.If, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.StartsWithWord("$elseif", true))
                {
                    var image = end.Take(7);
                    end = end.Skip(7);
                    tokens.Add(new Token(TokenKind.ElseIf, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.StartsWithWord("$else", true))
                {
                    var image = end.Take(5);
                    end = end.Skip(5);
                    tokens.Add(new Token(TokenKind.Else, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.StartsWithWord("$foreach", true))
                {
                    var image = end.Take(8);
                    end = end.Skip(8);
                    tokens.Add(new Token(TokenKind.ForEach, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.StartsWithWord("$next", true))
                {
                    var image = end.Take(5);
                    end = end.Skip(5);
                    tokens.Add(new Token(TokenKind.Next, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.StartsWithWord("$end", true))
                {
                    var image = end.Take(4);
                    end = end.Skip(4);
                    tokens.Add(new Token(TokenKind.End, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.StartsWithWord("$first", true))
                {
                    var image = end.Take(6);
                    end = end.Skip(6);
                    tokens.Add(new Token(TokenKind.First, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.StartsWithWord("$last", true))
                {
                    var image = end.Take(5);
                    end = end.Skip(5);
                    tokens.Add(new Token(TokenKind.Last, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }
                
                if (end.StartsWithWord("$middle", true))
                {
                    var image = end.Take(7);
                    end = end.Skip(7);
                    tokens.Add(new Token(TokenKind.Middle, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }
                
                if (end.StartsWithWord("$odd", true))
                {
                    var image = end.Take(4);
                    end = end.Skip(4);
                    tokens.Add(new Token(TokenKind.Odd, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }
                
                if (end.StartsWithWord("$even", true))
                {
                    var image = end.Take(5);
                    end = end.Skip(5);
                    tokens.Add(new Token(TokenKind.Even, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.StartsWith("."))
                {
                    var image = end.Take(1);
                    end = end.Skip(1);
                    tokens.Add(new Token(TokenKind.Dot, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.StartsWith(":"))
                {
                    var image = end.Take(1);
                    end = end.Skip(1);
                    tokens.Add(new Token(TokenKind.Colon, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                if (end.IsWordCharacter())
                {
                    var image = end.TakeWhile(c => c.IsWordCharacter());
                    end = end.Skip(image.Length);
                    tokens.Add(new Token(TokenKind.Word, image));
                    end = end.SkipWhitespace(true);
                    continue;
                }

                throw new DomException(end, "Unexpected character in macro");
            }

            if (!end.StartsWith("}}"))
                throw new DomException(end, "Unterminated macro");
            end = end.Skip(2);
            if (isLineMacro)
            {
                end = end.SkipWhitespace(true);
                if (end.IsAtEndOfLine())
                    end = end.FindEndOfLine(true);
                else
                {
                    if (hasTrivia)
                    {
                        macro = default;
                        return false;
                    }

                    isLineMacro = false;
                }
            }

            ptr = end;
            macro = new IntermediateMacro(tokens.ToImmutable(), isLineMacro);
            return true;
        }

        public static bool TryParse(ref TextPtr ptr, TokenKind id, out IntermediateMacro macro)
        {
            var end = ptr;
            if (!TryParse(ref end, out var tmp))
            {
                macro = default;
                return false;
            }

            if (tmp.Tokens.Length < 1 || tmp.Tokens[0].Kind != id)
            {
                macro = default;
                return false;
            }

            ptr = end;
            macro = tmp;
            return true;
        }

        public bool IsKind(TokenKind kind) => Tokens.Length >= 1 && Tokens[0].Kind == kind;
    }
}