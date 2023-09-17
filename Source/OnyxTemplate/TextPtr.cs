// OnyxTemplate
// 
// Copyright 2023 Morten A. Lyrstad

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Mal.OnyxTemplate
{
    /// <summary>
    ///     Points to a specific location within a text string.
    /// </summary>
    [DebuggerDisplay("{ToDebugString(),nq}")]
    readonly struct TextPtr
    {
        /// <summary>
        ///     Advances the pointer by one.
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static TextPtr operator ++(in TextPtr ptr) => new TextPtr(ptr.Text, ptr.Index + 1);

        /// <summary>
        ///     Retreats the pointer by one.
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static TextPtr operator --(in TextPtr ptr) => new TextPtr(ptr.Text, ptr.Index - 1);

        /// <summary>
        ///     Advances the pointer by a given amount.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static TextPtr operator +(in TextPtr ptr, int n) => new TextPtr(ptr.Text, ptr.Index + n);

        /// <summary>
        ///     Retreats the pointer by a given amount.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static TextPtr operator -(in TextPtr ptr, int n) => new TextPtr(ptr.Text, ptr.Index - n);

        /// <summary>
        ///     The text this pointer is referencing.
        /// </summary>
        public readonly string Text;

        /// <summary>
        ///     The location within the <see cref="Text" /> this pointer is referencing.
        /// </summary>
        public readonly int Index;

        /// <summary>
        ///     Creates a <see cref="TextPtr" />.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        public TextPtr(string text, int index = 0)
        {
            Text = text;
            Index = Math.Max(-1, Math.Min(text.Length, index));
        }

        /// <summary>
        ///     Gets the character pointed at in the referenced text. Returns a null character '\0' if outside of its range.
        /// </summary>
        public char Char => IsBeforeStart() || IsPastEnd() ? '\0' : Text[Index];

        /// <summary>
        ///     Gets the line number and position of this pointer in the text source, tanking the desired tab length into account
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public (int ln, int ch) GetPosition(int tabLength)
        {
            if (string.IsNullOrEmpty(Text))
                return (1, 1);

            if (tabLength < 1)
                throw new ArgumentException("Tab length must be greater than 0.", nameof(tabLength));

            var ln = 1;
            var ch = 1;

            var index = Math.Max(0, Math.Min(Text.Length - 1, Index));
            for (var i = 0; i < index; i++)
            {
                switch (Text[i])
                {
                    case '\n':
                        ln++;
                        ch = 1;
                        break;
                    case '\t':
                        ch += tabLength - (ch - 1) % tabLength;
                        break;
                    default:
                        ch++;
                        break;
                }
            }

            return (ln, ch);
        }

        /// <summary>
        ///     Gets the line number and position of this pointer in the text source.
        /// </summary>
        /// <returns></returns>
        public (int ln, int ch) GetPosition()
        {
            if (string.IsNullOrEmpty(Text))
                return (1, 1);

            var ln = 1;
            var ch = 1;

            var index = Math.Max(0, Math.Min(Text.Length - 1, Index));
            for (var i = 0; i < index; i++)
            {
                if (Text[i] == '\n')
                {
                    ln++;
                    ch = 1;
                }
                else
                    ch++;
            }

            return (ln, ch);
        }

        /// <summary>
        ///     Determines if the pointer is unset.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty() => Text == null;

        /// <summary>
        ///     Determines if the pointer has advanced past the end of the referenced text.
        /// </summary>
        /// <returns></returns>
        public bool IsPastEnd() => Index >= Text.Length;

        /// <summary>
        ///     Determines if the pointer has retreated past the beginning of the referenced text.
        /// </summary>
        /// <returns></returns>
        public bool IsBeforeStart() => Index < 0;

        /// <summary>
        ///     Returns a pointer that has skipped all whitespace from the position of this pointer.
        /// </summary>
        /// <returns></returns>
        public TextPtr SkipWhitespace(bool stopOnNewLine)
        {
            var index = Index;
            if (stopOnNewLine)
            {
                while (index < Text.Length && !IsNewLine(Text, index) && Text[index] != '\n' && char.IsWhiteSpace(Text[index]))
                    index++;
            }
            else
            {
                while (index < Text.Length && char.IsWhiteSpace(Text[index]))
                    index++;
            }

            return new TextPtr(Text, index);
        }

        /// <summary>
        ///     Returns a pointer that has skipped all the given characters from the position of this pointer.
        /// </summary>
        /// <param name="characters"></param>
        /// <returns></returns>
        public TextPtr Skip(params char[] characters)
        {
            var index = Index;
            while (index < Text.Length && Array.IndexOf(characters, Text[index]) >= 0) index++;

            return new TextPtr(Text, index);
        }

        /// <summary>
        ///     Returns a pointer which is located where one of the provided characters can be found. Returns a pointer that is
        ///     <see cref="IsPastEnd" /> if nothing is found.
        /// </summary>
        /// <param name="characters"></param>
        /// <returns></returns>
        public TextPtr Find(params char[] characters)
        {
            var index = Index;
            while (index < Text.Length && Array.IndexOf(characters, Text[index]) < 0) index++;

            return new TextPtr(Text, index);
        }

        /// <summary>
        /// Attempts to find the given pattern, but only within the current line of the text (stops at newline).
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public TextPtr FindInLine(string pattern)
        {
            var ptr = this;
            while (!ptr.IsPastEnd())
            {
                if (ptr.StartsWith(pattern))
                    return ptr;
                if (ptr.IsNewLine())
                    break;
            }

            return new TextPtr(Text, Text.Length);
        }
        
        /// <summary>
        ///     Takes a string segment from the location of this pointer, to the location of another.
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public StringSegment TakeUntil(TextPtr end) => new StringSegment(Text, Index, end.Index - Index);

        /// <summary>
        ///     Takes a string segment from the location of this pointer to the end of the origin string.
        /// </summary>
        /// <returns></returns>
        public StringSegment TakeRest() => new StringSegment(Text, Index, Text.Length - Index);

        /// <summary>
        ///     Determines if the next characters in the string matches one of the alternatives. Returns <c>-1</c> if none were
        ///     found.
        /// </summary>
        /// <param name="alternatives"></param>
        /// <param name="after"></param>
        /// <param name="caseInsensitive"></param>
        /// <returns></returns>
        public int StartsWithOneOf(IReadOnlyList<string> alternatives, out TextPtr after, bool caseInsensitive = false)
        {
            for (var i = 0; i < alternatives.Count; i++)
            {
                var alternative = alternatives[i];
                if (!StartsWith(alternative)) continue;

                after = this + alternative.Length;
                return i;
            }

            after = this;
            return -1;
        }

        /// <summary>
        ///     Determines if the next characters in the string matches the given string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caseInsentitive"></param>
        /// <returns></returns>
        public bool StartsWith(string text, bool caseInsentitive = false)
        {
            var n = this;
            if (caseInsentitive)
            {
                foreach (var ch in text)
                {
                    if (char.ToUpper(n.Char) != char.ToUpper(ch)) return false;

                    n++;
                }
            }
            else
            {
                foreach (var ch in text)
                {
                    if (n.Char != ch) return false;

                    n++;
                }
            }

            return true;
        }

        /// <summary>
        ///     Attempts to find the given substring.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caseInsensitive"></param>
        /// <returns></returns>
        public TextPtr Find(string text, bool caseInsensitive = false)
        {
            var flag = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var idx = Text.IndexOf(text, Index, flag);
            return idx == -1 ? new TextPtr(Text, Text.Length) : new TextPtr(Text, idx);
        }

        /// <summary>
        ///     Determines whether this pointer is currently situated at the start of a line, meaning either the start of the
        ///     string, or directly after a newline.
        /// </summary>
        /// <returns></returns>
        public bool IsAtStartOfLine()
        {
            if (string.IsNullOrEmpty(Text)) return true;

            return Index == 0 || (this - 1).Char == '\n';
        }

        /// <summary>
        /// Determines whether this pointer is currently situated at the end of a line, meaning either at the end of the string,
        /// or directly ahead of a newline.
        /// </summary>
        /// <returns></returns>
        public bool IsAtEndOfLine()
        {
            if (string.IsNullOrEmpty(Text)) return true;
            return Index >= Text.Length || IsNewLine();
        }

        /// <summary>
        ///     Determines if this pointer is currently at a newline.
        /// </summary>
        /// <returns></returns>
        public bool IsNewLine() => Char == '\n' || StartsWith("\r\n");

        /// <summary>
        ///     Finds the end of the current line.
        /// </summary>
        /// <param name="skip"></param>
        /// <returns></returns>
        public TextPtr FindEndOfLine(bool skip = false)
        {
            var end = this;
            while (!end.IsPastEnd())
            {
                if (end.Char == '\n')
                {
                    if (skip) end++;

                    return end;
                }

                if (end.StartsWith("\r\n"))
                {
                    if (skip) end += 2;

                    return end;
                }

                end++;
            }

            return end;
        }

        /// <summary>
        /// Gets the current position in a '(ln, ch)' format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Text == null)
                return "{empty}";
            var (ln, ch) = GetPosition();
            return $"({ln}, {ch})";
        }

        static bool IsNewLine(string text, int index)
        {
            if (index >= text.Length)
                return false;
            if (text[index] == '\n')
                return true;
            if (index + 1 >= text.Length)
                return true;
            if (text[index] == '\r' && text[index + 1] == '\n')
                return true;
            return false;
        }

        string ToDebugString()
        {
            if (IsEmpty()) return "{empty}";

            var builder = new StringBuilder();
            var (ln, ch) = GetPosition();
            builder.Append("(").Append(ln).Append(", ").Append(ch).Append(") ");

            if (Index > 0) builder.Append("{...}");

            var n = 40 - builder.Length;

            if (Text.Length - Index > n)
            {
                builder.Append(Text, Index, n - 5);
                builder.Append("{...}");
            }
            else
                builder.Append(Text, Index, Text.Length - Index);

            for (var i = builder.Length - 1; i >= 0; i--)
            {
                switch (builder[i])
                {
                    case '\r':
                        builder.Remove(i, 0);
                        builder.Insert(i, "\r");
                        break;
                    case '\n':
                        builder.Remove(i, 0);
                        builder.Insert(i, "\n");
                        break;
                    case '\t':
                        builder.Remove(i, 0);
                        builder.Insert(i, "\t");
                        break;
                }
            }
            return builder.ToString();
        }
    }
}