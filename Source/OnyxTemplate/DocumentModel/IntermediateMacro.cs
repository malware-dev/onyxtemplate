// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.Collections.Immutable;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     An intermediate macro that is used during parsing.
    /// </summary>
    class IntermediateMacro
    {
        IntermediateMacro(ImmutableArray<Token> tokens, bool isLineMacro)
        {
            Tokens = tokens;
            IsLineMacro = isLineMacro;
        }

        /// <summary>
        ///     The tokens that make up the macro.
        /// </summary>
        public ImmutableArray<Token> Tokens { get; }

        /// <summary>
        ///     Whether the macro takes up the entire line.
        /// </summary>
        public bool IsLineMacro { get; }

        /// <summary>
        ///     What kind of macro this is, as determined by the first token.
        /// </summary>
        public TokenKind Kind => Tokens.Length >= 1 ? Tokens[0].Kind : TokenKind.None;

        /// <summary>
        ///     Gets a token at the specified index.
        /// </summary>
        /// <param name="index"></param>
        public Token this[int index]
        {
            get
            {
                if (index < 0 || index >= Tokens.Length)
                    return default;
                return Tokens[index];
            }
        }

        /// <summary>
        ///     Attempts to parse a macro from the specified pointer.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="macro"></param>
        /// <returns></returns>
        /// <exception cref="DomException"></exception>
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

                if (end.StartsWith("\"") || end.StartsWith("'"))
                {
                    var quote = end.Char;
                    end = end.Skip(1);
                    var image = end.TakeWhile(c => c.Char != quote);
                    end = end.Skip(image.Length);
                    if (end.Char == quote)
                        end = end.Skip(1);
                    else
                        throw new DomException(end, 1, "Unterminated string");
                    tokens.Add(new Token(TokenKind.String, image));
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

                throw new DomException(end, 1, "Unexpected character in macro");
            }

            if (!end.StartsWith("}}"))
                throw new DomException(end, 1, "Unterminated macro");
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

        /// <summary>
        ///     Attempts to parse a macro from the specified pointer, and checks if it is of the specified kind.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="id"></param>
        /// <param name="macro"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     Determines if this macro is of the specified kind.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public bool IsKind(TokenKind kind) => Tokens.Length >= 1 && Tokens[0].Kind == kind;
    }
}