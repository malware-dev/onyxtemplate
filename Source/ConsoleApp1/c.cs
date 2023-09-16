#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
using System.Text;

namespace ConsoleApp1
{
    public abstract class TestTemplateBasee
    {
        protected class Writer
        {
            readonly StringBuilder _buffer = new StringBuilder(1024);
            static int FindEndOfLine(string input, int start)
            {
                var index = input.IndexOf('\n', start);
                return index < 0? input.Length : index + 1;
            }
            public string Indentation { get; set; } = "";
            void Indent()
            {
                _buffer.Append(Indentation);
            }
            public void Append(string input)
            {
                if (string.IsNullOrEmpty(input)) return;
                if (_buffer.Length > 0 && _buffer[_buffer.Length - 1] == '\n')
                    Indent();
                var start = 0;
                var end = FindEndOfLine(input, start);
                _buffer.Append(input, start, end);
                while (end < input.Length)
                {
                    start = end;
                    end = FindEndOfLine(input, start);
                    Indent();
                    _buffer.Append(input, start, end - start);
                }
            }
            public void AppendLine(string input = null)
            {
                Append(input);
                _buffer.AppendLine();
            }
            public override string ToString() => _buffer.ToString();
        }
        protected virtual string GetThings() { return string.Empty; }
        public override string ToString()
        {
            var builder = new Writer();
            builder.Append(@"bla bla bla ");
            builder.Indentation = "            ";
            builder.Append(GetThings());
            builder.Indentation = "";
            builder.Append(@" and stuffiesvcxcvsdfaasdfasd12234rwerw
");
            return builder.ToString();
        }
    }
}