// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.IO;
using System.Linq;

namespace Mal.OnyxTemplate
{
    class ScopedWriter
    {
        string _indentation;

        public ScopedWriter(TextWriter writer, int indentation)
        {
            Writer = writer;
            Indentation = indentation;
            _indentation = new string(' ', indentation * 4);
            _needsIndent = true;
        }

        bool _needsIndent;
        public TextWriter Writer { get; }
        public int Indentation { get; private set; }

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
            Writer.WriteLine();
            _needsIndent = true;
            return this;
        }

        public ScopedWriter AppendLine()
        {
            Writer.WriteLine();
            _needsIndent = true;
            return this;
        }

        static readonly string[] NewLines = { "\r\n", "\n" };

        public ScopedWriter Append(string value)
        {
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\r':
                        continue;
                    case '\n':
                        Writer.WriteLine();
                        _needsIndent = true;
                        continue;
                    default:
                        if (_needsIndent)
                        {
                            Writer.Write(_indentation);
                            _needsIndent = false;
                        }
                        Writer.Write(ch);
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