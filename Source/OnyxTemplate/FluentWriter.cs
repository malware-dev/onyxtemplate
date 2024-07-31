// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mal.OnyxTemplate
{
    /// <summary>
    ///     A class that helps with writing code in a fluent manner, specifically for generating code.
    /// </summary>
    class FluentWriter
    {
        readonly Stack<TextWriter> _blockStack = new Stack<TextWriter>();
        string _indentation;
        bool _needsIndent;
        TextWriter _writer;

        /// <summary>
        ///     Creates a new instance of <see cref="FluentWriter" />.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="indentation"></param>
        public FluentWriter(TextWriter writer, int indentation)
        {
            Writer = _writer = writer;
            Indentation = indentation;
            _indentation = new string(' ', indentation * 4);
            _needsIndent = true;
        }

        /// <summary>
        ///     The base writer that is being written to.
        /// </summary>
        public TextWriter Writer { get; }

        /// <summary>
        ///     The current indentation level.
        /// </summary>
        public int Indentation { get; private set; }

        /// <summary>
        ///     Begins a new code block: { ... }. Use <see cref="EndBlock" /> to end the block.
        /// </summary>
        /// <returns></returns>
        public FluentWriter BeginBlock()
        {
            var block = new StringWriter();
            _blockStack.Push(_writer);
            _writer = block;
            return this;
        }

        static bool IsMultiLine(string value)
        {
            var endsWithNewLine = value.EndsWith("\n");
            var newlineCount = value.Count(c => c == '\n');
            return newlineCount > 1 || newlineCount == 1 && !endsWithNewLine;
        }

        /// <summary>
        ///     Ends the current code block. If <paramref name="optionalBraces" /> is true, the braces are omitted if the block is
        ///     a single line. Otherwise, the braces are always included.
        /// </summary>
        /// <param name="optionalBraces"></param>
        /// <returns></returns>
        public FluentWriter EndBlock(bool optionalBraces = false)
        {
            var blockContent = _writer.ToString();
            _writer = _blockStack.Pop();

            if (!optionalBraces || IsMultiLine(blockContent))
            {
                AppendLine("{").Indent();
                Append(blockContent);
                Unindent().AppendLine("}");
            }
            else
            {
                Indent()
                    .Append(blockContent)
                    .Unindent();
            }

            return this;
        }

        /// <summary>
        ///     Indents the following code by <paramref name="n" /> levels (default 1).
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public FluentWriter Indent(uint n = 1)
        {
            Indentation += (int)n;
            _indentation = new string(' ', Indentation * 4);
            return this;
        }

        /// <summary>
        ///     Unindents the following code by <paramref name="n" /> levels (default 1).
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public FluentWriter Unindent(uint n = 1)
        {
            Indentation = Math.Max(0, Indentation - (int)n);
            _indentation = new string(' ', Indentation * 4);
            return this;
        }

        /// <summary>
        ///     Appends a line of code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentWriter AppendLine(string value)
        {
            Append(value);
            _writer.WriteLine();
            _needsIndent = true;
            return this;
        }

        /// <summary>
        ///     Adds an empty new line.
        /// </summary>
        /// <returns></returns>
        public FluentWriter AppendLine()
        {
            _writer.WriteLine();
            _needsIndent = true;
            return this;
        }

        /// <summary>
        ///     Appends code.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public FluentWriter Append(string value)
        {
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\r':
                        continue;
                    case '\n':
                        _writer.WriteLine();
                        _needsIndent = true;
                        continue;
                    default:
                        if (_needsIndent)
                        {
                            _writer.Write(_indentation);
                            _needsIndent = false;
                        }

                        _writer.Write(ch);
                        continue;
                }
            }

            return this;
        }

        /// <summary>
        ///     Appends code if the condition is true.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="whenTrue"></param>
        /// <param name="whenFalse"></param>
        /// <returns></returns>
        public FluentWriter AppendIf(bool condition, string whenTrue, string whenFalse = null)
        {
            if (condition)
                Append(whenTrue);
            else if (whenFalse != null)
                Append(whenFalse);
            return this;
        }

        /// <summary>
        ///     Appends a line of code if the condition is true.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="whenTrue"></param>
        /// <param name="whenFalse"></param>
        /// <returns></returns>
        public FluentWriter AppendLineIf(bool condition, string whenTrue, string whenFalse = null)
        {
            if (condition)
                AppendLine(whenTrue);
            else if (whenFalse != null)
                AppendLine(whenFalse);
            return this;
        }
    }
}