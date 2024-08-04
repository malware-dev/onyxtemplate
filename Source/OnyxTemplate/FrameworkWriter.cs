// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.IO;

namespace Mal.OnyxTemplate
{
    /// <summary>
    /// A writer used to generate the framework code, which is shared between all templates.
    /// </summary>
    public class FrameworkWriter
    {
        readonly StringWriter _writer;

        public FrameworkWriter(StringWriter writer)
        {
            _writer = writer;
        }

        public void Write()
        {
            var file = new FluentWriter(_writer, 0);
            file.AppendLine("using System;")
                .AppendLine("using System.Text;")
                .AppendLine("using System.Collections.Generic;")
                .AppendLine("#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.")
                .AppendLine("#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member.")
                .AppendLine("namespace Mal.OnyxTemplate")
                .AppendLine("{")
                .AppendLine("    public abstract class TextTemplate")
                .AppendLine("    {")
                .AppendLine("        static readonly string[] NewLines = new string[] { \"\\r\\n\", \"\\n\" };")
                .AppendLine("        static readonly char[] NewLineChars = new char[] { '\\r', '\\n' };")
                .AppendLine("        protected class Writer")
                .AppendLine("        {")
                .AppendLine("            readonly StringBuilder _buffer = new StringBuilder(1024);")
                .AppendLine("            protected static IEnumerable<string> LinesOf(string input)")
                .AppendLine("            {")
                .AppendLine("                var start = 0;")
                .AppendLine("                var end = input.IndexOf('\\n', start);")
                .AppendLine("                if (end < 0)")
                .AppendLine("                {")
                .AppendLine("                    yield return input;")
                .AppendLine("                    yield break;")
                .AppendLine("                }")
                .AppendLine("                while (end >= 0)")
                .AppendLine("                {")
                .AppendLine("                    var br = 1;")
                .AppendLine("                    if (end > 0 && input[end - 1] == '\\r')")
                .AppendLine("                    {")
                .AppendLine("                        br = 2;")
                .AppendLine("                        end--;")
                .AppendLine("                    }")
                .AppendLine("                    yield return input.Substring(start, end - start);")
                .AppendLine("                    start = end + br;")
                .AppendLine("                    end = input.IndexOf('\\n', start);")
                .AppendLine("                }")
                .AppendLine("                yield return input.Substring(start);")
                .AppendLine("            }")
                .AppendLine("            string GetIndentStr()")
                .AppendLine("            {")
                .AppendLine("                if (_buffer.Length == 0 || _buffer[_buffer.Length - 1] == '\\n')")
                .AppendLine("                    return string.Empty;")
                .AppendLine("                var builder = new StringBuilder();")
                .AppendLine("                var index = _buffer.Length - 1;")
                .AppendLine("                while (index >= 0 && _buffer[index] != '\\n')")
                .AppendLine("                {")
                .AppendLine("                    builder.Insert(0, _buffer[index] == '\\t'? '\\t' : ' ');")
                .AppendLine("                    index--;")
                .AppendLine("                }")
                .AppendLine("                return builder.ToString();")
                .AppendLine("            }")
                .AppendLine("            public void Append(string input, bool indent = false)")
                .AppendLine("            {")
                .AppendLine("                if (string.IsNullOrEmpty(input)) return;")
                .AppendLine("                string indentStr = indent? GetIndentStr() : string.Empty;")
                .AppendLine("                int n = 0;")
                .AppendLine("                foreach (var line in LinesOf(input))")
                .AppendLine("                {")
                .AppendLine("                    if (n++ > 0) _buffer.AppendLine();")
                .AppendLine("                    if (indent && _buffer.Length > 0 && _buffer[_buffer.Length - 1] == '\\n')")
                .AppendLine("                        _buffer.Append(indentStr);")
                .AppendLine("                    _buffer.Append(line);")
                .AppendLine("                }")
                .AppendLine("            }")
                .AppendLine("            public void AppendLine(string input = null, bool indent = false)")
                .AppendLine("            {")
                .AppendLine("                Append(input, indent);")
                .AppendLine("                _buffer.AppendLine();")
                .AppendLine("            }")
                .AppendLine("            public override string ToString() => _buffer.ToString();")
                .AppendLine("        }")
                .AppendLine("        protected class State")
                .AppendLine("        {")
                .AppendLine("            int _index;")
                .AppendLine("            public State(int count, State parent = null)")
                .AppendLine("            {")
                .AppendLine("                Parent = parent;")
                .AppendLine("                Count = count;")
                .AppendLine("            }")
                .AppendLine("            public State Parent { get; }")
                .AppendLine("            public int Count { get; }")
                .AppendLine("            public int Index")
                .AppendLine("            {")
                .AppendLine("                get { return _index; }")
                .AppendLine("                set")
                .AppendLine("                {")
                .AppendLine("                    _index = value;")
                .AppendLine("                    First = value == 0;")
                .AppendLine("                    Last = value == Count - 1;")
                .AppendLine("                    Even = value % 2 == 0;")
                .AppendLine("                }")
                .AppendLine("            }")
                .AppendLine("            public bool First { get; private set; }")
                .AppendLine("            public bool Last { get; private set; }")
                .AppendLine("            public bool Even { get; private set;}")
                .AppendLine("            public bool Middle => !First && !Last;")
                .AppendLine("            public bool Odd => !Even;")
                .AppendLine("        }")
                .AppendLine("    }")
                .AppendLine("}");
        }

        public override string ToString() => _writer.ToString();
    }
}