// OnyxTemplate
// 
// Copyright 2023 Morten A. Lyrstad

using System;
using System.Collections.Generic;

namespace Mal.OnyxTemplate
{
    /// <summary>
    /// A segment of a string.
    /// </summary>
    struct StringSegment
    {
        static readonly char[] NewlineChars = { '\r', '\n' };

        public readonly string Text;
        public readonly int Start;
        public readonly int Length;
        string _cache;

        /// <summary>
        /// Creates a new <see cref="StringSegment"/> representing a full string.
        /// </summary>
        /// <param name="text"></param>
        public StringSegment(string text)
            : this(text, 0, text.Length)
        {
            _cache = text;
        }

        /// <summary>
        /// Creates a new <see cref="StringSegment"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        public StringSegment(string text, int start, int length)
        {
            Text = text;
            Start = start;
            Length = Math.Max(0, length);
            _cache = null;
        }

        /// <summary>
        /// Creates a new <see cref="StringSegment"/>.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public StringSegment(TextPtr start, TextPtr end) : this(start.Text, start.Index, end.Index - start.Index) { }

        /// <summary>
        /// Determines if this segment is empty.
        /// </summary>
        public bool IsEmpty => Text == null;

        /// <summary>
        /// Determines whether this segment has been cached into its own separate string.
        /// </summary>
        public bool IsCached => _cache != null;

        /// <summary>
        /// Returns the character at the relative index.
        /// </summary>
        /// <param name="i"></param>
        public char this[int i]
        {
            get
            {
                if (i < 0 || i >= Length)
                    return '\0';
                return Text[Start + i];
            }
        }

        /// <summary>
        /// Attempts to find the string-relative (i.e. not relative to the segment start) index of the given character.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public int IndexOf(char ch)
        {
            if (Length == 0)
                return -1;
            return Text.IndexOf(ch, Start, Length);
        }

        /// <summary>
        /// Attempts to find the string-relative (i.e. not relative to the segment start) index of the given character.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public int IndexOf(char ch, int start)
        {
            if (Length == 0)
                return -1;
            return Text.IndexOf(ch, Start + start, Length);
        }

        /// <summary>
        /// Attempts to find the string-relative (i.e. not relative to the segment start) index of any of the given characters.
        /// </summary>
        /// <param name="chars"></param>
        /// <returns></returns>
        public int IndexOfAny(char[] chars)
        {
            if (Length == 0)
                return -1;
            return Text.IndexOfAny(chars, Start, Length);
        }

        /// <summary>
        /// Compares this segment to another, in a case insensitive manner.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EqualsIgnoreCase(StringSegment other)
        {
            if (Length != other.Length)
                return false;
            var a = Text;
            var ai = Start;
            var b = other.Text;
            var bi = other.Start;
            for (var i = 0; i < Length; i++)
            {
                if (char.ToUpperInvariant(a[ai]) != char.ToUpperInvariant(b[bi]))
                    return false;
                ai++;
                bi++;
            }

            return true;
        }

        /// <summary>
        /// Gets this segment as a separate string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Length == 0)
                return "";
            return _cache ?? (_cache = Text.Substring(Start, Length));
        }

        /// <summary>
        /// Splits this segment into lines.
        /// </summary>
        /// <param name="lines"></param>
        public void GetLines(List<string> lines)
        {
            if (Length == 0)
                return;
            var str = Text;
            if (string.IsNullOrEmpty(str))
                return;
            var start = Start;
            var n = start + Length;
            lines.Clear();
            while (start < n)
            {
                var index = str.IndexOfAny(NewlineChars, start, n - start);
                if (index < 0)
                {
                    lines.Add(str.Substring(start, str.Length - start));
                    return;
                }

                lines.Add(str.Substring(start, index - start));
                start = index;
                if (start < str.Length && str[start] == '\r')
                    start++;
                if (start < str.Length && str[start] == '\n')
                    start++;
            }
        }

        /// <summary>
        /// Splits this segment into lines.
        /// </summary>
        /// <param name="lines"></param>
        public void GetLines(List<StringSegment> lines)
        {
            if (Length == 0)
                return;
            var str = Text;
            if (string.IsNullOrEmpty(str))
                return;
            var start = Start;
            var n = start + Length;
            lines.Clear();
            while (start < n)
            {
                var index = str.IndexOfAny(NewlineChars, start, n - start);
                if (index < 0)
                {
                    lines.Add(new StringSegment(str, start, str.Length - start));
                    return;
                }

                lines.Add(new StringSegment(str, start, index - start));
                start = index;
                if (start < str.Length && str[start] == '\r')
                    start++;
                if (start < str.Length && str[start] == '\n')
                    start++;
            }
        }
    }
}