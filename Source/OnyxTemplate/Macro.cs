// OnyxTemplate
// 
// Copyright 2023 Morten A. Lyrstad

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mal.OnyxTemplate
{
    /// <summary>
    ///     A macro definition of a .onyx template.
    /// </summary>
    [DebuggerDisplay("{ToDebugString(),nq}")]
    class Macro
    {
        // ReSharper disable once InconsistentNaming
        static long __idSrc;

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

        readonly bool _forceIndent;
        string _itemTypeName;
        string _sourceName;

        /// <summary>
        ///     Creates a new <see cref="Macro" />.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="name"></param>
        /// <param name="source"></param>
        /// <param name="isStateField"></param>
        /// <param name="not"></param>
        /// <param name="tags"></param>
        public Macro(MacroType type, TextPtr start, TextPtr end, string name, string source, bool isStateField, bool not, HashSet<string> tags)
        {
            Type = type;
            Start = start;
            End = end;
            Name = name;
            Source = source;
            IsStateField = isStateField;
            Tags = tags;
            IsNot = not;
            _forceIndent = tags?.Contains("indent") ?? false;
        }
        
        public long Id { get; } = ++__idSrc;

        /// <summary>
        ///     What type of macro this is.
        /// </summary>
        public MacroType Type { get; }

        /// <summary>
        /// For a <see cref="Type"/> <see cref="MacroType.If"/> or <see cref="MacroType.ElseIf"/>, denotes a negative check.
        /// </summary>
        public bool IsNot { get; set; }
        
        /// <summary>
        ///     The start point of this macro in the source .onyx text.
        /// </summary>
        public TextPtr Start { get; }

        /// <summary>
        ///     The end point of this macro in the source .onyx text.
        /// </summary>
        public TextPtr End { get; }

        /// <summary>
        ///     The name of this macro. Primarily used for the <see cref="MacroType.ForEach" /> macro type.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The name of the source for this macro - or the raw text for a <see cref="MacroType.Text" /> macro.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Whether <see cref="Source"/> refers to a state field rather than a data field.
        /// </summary>
        public bool IsStateField { get; }

        /// <summary>
        ///     A list of macro tags.
        /// </summary>
        public HashSet<string> Tags { get; }

        /// <summary>
        ///     Child macros for <see cref="MacroType.ForEach" /> type macros.
        /// </summary>
        public List<Macro> Macros { get; } = new List<Macro>();

        /// <summary>
        ///     The parent macro.
        /// </summary>
        public Macro Parent { get; set; }

        /// <summary>
        ///     The type name to use for complex macros (<see cref="MacroType.ForEach" /> type macros).
        /// </summary>
        /// <returns></returns>
        public string ItemTypeName()
        {
            if (_itemTypeName != null)
                return _itemTypeName;
            if (Parent == null || Parent.Type == MacroType.Root)
            {
                _itemTypeName = CSharpify(Source) + "ItemBase";
                return _itemTypeName;
            }

            var builder = new StringBuilder(1024);
            builder.Append(CSharpify(Source) + "ItemBase");
            var macro = Parent;
            while (macro.Type != MacroType.Root)
            {
                builder.Insert(0, CSharpify(macro.Source));
                macro = macro.Parent;
            }

            _itemTypeName = builder.ToString();
            return _itemTypeName;
        }

        /// <summary>
        ///     The <see cref="SourceName" /> in a <see cref="CSharpify" /> processed way.
        /// </summary>
        /// <returns></returns>
        public string SourceName()
        {
            if (_sourceName != null)
                return _sourceName;
            _sourceName = CSharpify(Source);
            return _sourceName;
        }

        /// <summary>
        ///     A flat list of all the descendants of this macro tree.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Macro> Descendants()
        {
            foreach (var macro in Macros)
            {
                yield return macro;
                foreach (var child in macro.Descendants())
                    yield return child;
            }
        }

        /// <summary>
        ///     Whether this is a simple (single-macro) list.
        /// </summary>
        /// <returns></returns>
        public bool IsSimpleList()
        {
            return Macros.All(m => m.Type == MacroType.Text) || Macros.All(m => m.Type == MacroType.Text ||
                                                                                m.Type == MacroType.Ref &&
                                                                                string.Equals(m.Source,
                                                                                    Name,
                                                                                    StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Generates the indentation required for the position of this macro.
        /// </summary>
        /// <returns></returns>
        public string GenerateIndent()
        {
            var builder = new StringBuilder(1024);
            var ptr = Start;
            while (!ptr.IsAtStartOfLine())
            {
                builder.Insert(0, ptr.Char == '\t' ? '\t' : ' ');
                ptr--;
            }

            return builder.ToString();
        }

        /// <summary>
        ///     Whether indentation should be forced even if indentation not enabled globally.
        /// </summary>
        /// <returns></returns>
        public bool ForceIndent() => _forceIndent;

        string ToDebugString()
        {
            var builder = new StringBuilder();
            builder.Append(Type).Append(" ").Append(Source);
            return builder.ToString();
        }
    }
}