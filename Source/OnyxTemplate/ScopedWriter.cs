// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mal.OnyxTemplate
{
    class ScopedWriter
    {

        static readonly string[] NewLines = { "\r\n", "\n" };
        TextWriter _writer;
        string _indentation;
        readonly Stack<TextWriter> _blockStack = new Stack<TextWriter>();
        bool _needsIndent;

        public ScopedWriter(TextWriter writer, int indentation)
        {
            Writer = _writer = writer;
            Indentation = indentation;
            _indentation = new string(' ', indentation * 4);
            _needsIndent = true;
        }

        public TextWriter Writer { get; }
        public int Indentation { get; private set; }

        public ScopedWriter BeginBlock()
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
            return newlineCount > 1 || (newlineCount == 1 && !endsWithNewLine);
        }
        
        public ScopedWriter EndBlock(bool optionalBraces = false)
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

        public ScopedWriter Indent(uint n = 1)
        {
            Indentation += (int)n;
            _indentation = new string(' ', Indentation * 4);
            return this;
        }

        public ScopedWriter Unindent(uint n = 1)
        {
            Indentation = Math.Max(0, Indentation - (int)n);
            _indentation = new string(' ', Indentation * 4);
            return this;
        }

        public ScopedWriter AppendLine(string value)
        {
            Append(value);
            _writer.WriteLine();
            _needsIndent = true;
            return this;
        }

        public ScopedWriter AppendLine()
        {
            _writer.WriteLine();
            _needsIndent = true;
            return this;
        }

        public ScopedWriter Append(string value)
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

        public ScopedWriter AppendIf(bool condition, string whenTrue, string whenFalse = null)
        {
            if (condition)
                Append(whenTrue);
            else if (whenFalse != null)
                Append(whenFalse);
            return this;
        }

        public ScopedWriter AppendLineIf(bool condition, string whenTrue, string whenFalse = null)
        {
            if (condition)
                AppendLine(whenTrue);
            else if (whenFalse != null)
                AppendLine(whenFalse);
            return this;
        }
    }
}