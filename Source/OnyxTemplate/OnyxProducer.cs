// OnyxTemplate
// 
// Copyright 2023 Morten A. Lyrstad

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Mal.OnyxTemplate
{
    /// <summary>
    ///     Generates the source code for the runtime template of a single .onyx file.
    /// </summary>
    static class OnyxProducer
    {
        static readonly string[] KnownStates =
        {
            "first",
            "last",
            "middle",
            "odd",
            "even"
        };

        /// <summary>
        ///     Generates the source code for the runtime template of a single .onyx file.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceText"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void GenerateOnyxSource(TemplateContext context, AdditionalText sourceText)
        {
            var className = Macro.CSharpify(Path.GetFileNameWithoutExtension(sourceText.Path) + "Base");
            var start = new TextPtr(sourceText.GetText()?.ToString() ?? "");
            var end = start;

            void pushText()
            {
                if (end.Index > start.Index)
                {
                    var text = start.TakeUntil(end).ToString();
                    context.Add(new Macro(MacroType.Text, start, end, null, text, false, false, null));
                }

                start = end;
            }

            while (!end.IsPastEnd())
            {
                var after = end;
                if (TryReadMacro(ref after, out var macro))
                {
                    pushText();
                    start = end = after;
                    context.Add(macro);
                    continue;
                }

                end++;
            }

            pushText();

            var builder = new StringBuilder();
            builder.AppendLine(
                "#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.");
            builder.AppendLine("using System.Text;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine();
            builder.Append("namespace ").AppendLine(context.RootNamespace ?? "OnyxTemplates");
            builder.AppendLine("{");
            builder.Append("    ").Append(context.PublicClass ? "public " : "internal ").Append("abstract class ")
                .AppendLine(className);
            builder.AppendLine("    {");
            var indentation = 2;

            void indent()
            {
                builder.Append(' ', indentation * 4);
            }

            indent();
            builder.AppendLine("protected class Writer");
            indent();
            builder.AppendLine("{");
            indent();
            builder.AppendLine("    readonly StringBuilder _buffer = new StringBuilder(1024);");
            indent();
            builder.AppendLine("    static int FindEndOfLine(string input, int start)");
            indent();
            builder.AppendLine("    {");
            indent();
            builder.AppendLine("        var index = input.IndexOf('\\n', start);");
            indent();
            builder.AppendLine("        return index < 0? input.Length : index + 1;");
            indent();
            builder.AppendLine("    }");
            indent();
            builder.AppendLine("    public string Indentation { get; set; } = \"\";");
            indent();
            builder.AppendLine("    void Indent()");
            indent();
            builder.AppendLine("    {");
            indent();
            builder.AppendLine("        _buffer.Append(Indentation);");
            indent();
            builder.AppendLine("    }");
            indent();
            builder.AppendLine("    public void Append(string input)");
            indent();
            builder.AppendLine("    {");
            indent();
            builder.AppendLine("        if (string.IsNullOrEmpty(input)) return;");
            indent();
            builder.AppendLine("        if (_buffer.Length > 0 && _buffer[_buffer.Length - 1] == '\\n')");
            indent();
            builder.AppendLine("            Indent();");
            indent();
            builder.AppendLine("        var start = 0;");
            indent();
            builder.AppendLine("        var end = FindEndOfLine(input, start);");
            indent();
            builder.AppendLine("        _buffer.Append(input, start, end);");
            indent();
            builder.AppendLine("        while (end < input.Length)");
            indent();
            builder.AppendLine("        {");
            indent();
            builder.AppendLine("            start = end;");
            indent();
            builder.AppendLine("            end = FindEndOfLine(input, start);");
            indent();
            builder.AppendLine("            Indent();");
            indent();
            builder.AppendLine("            _buffer.Append(input, start, end - start);");
            indent();
            builder.AppendLine("        }");
            indent();
            builder.AppendLine("    }");
            indent();
            builder.AppendLine("    public void AppendLine(string input = null)");
            indent();
            builder.AppendLine("    {");
            indent();
            builder.AppendLine("        Append(input);");
            indent();
            builder.AppendLine("        _buffer.AppendLine();");
            indent();
            builder.AppendLine("    }");
            indent();
            builder.AppendLine("    public override string ToString() => _buffer.ToString();");
            indent();
            builder.AppendLine("}");

            indent();
            builder.AppendLine("protected struct State");
            indent();
            builder.AppendLine("{");
            indent();
            builder.AppendLine("    public State(bool first, bool last, bool even)");
            indent();
            builder.AppendLine("    {");
            indent();
            builder.AppendLine("        First = first;");
            indent();
            builder.AppendLine("        Last = last;");
            indent();
            builder.AppendLine("        Even = even;");
            indent();
            builder.AppendLine("    }");
            indent();
            builder.AppendLine("    public bool First { get; }");
            indent();
            builder.AppendLine("    public bool Last { get; }");
            indent();
            builder.AppendLine("    public bool Even { get; }");
            indent();
            builder.AppendLine("    public bool Middle => !First && !Last;");
            indent();
            builder.AppendLine("    public bool Odd => !Even;");
            indent();
            builder.AppendLine("}");
            indent();
            builder.AppendLine("delegate void ListWriterFn<T>(Writer writer, T item, in State state);");
            indent();
            builder.AppendLine("static void WriteListItem<T>(IEnumerable<T> items, Writer writer, ListWriterFn<T> writeFn)");
            indent();
            builder.AppendLine("{");
            indent();
            builder.AppendLine("    using (var enumerator = items.GetEnumerator())");
            indent();
            builder.AppendLine("    {");
            indent();
            builder.AppendLine("        if (!enumerator.MoveNext())");
            indent();
            builder.AppendLine("            return;");
            indent();
            builder.AppendLine();
            indent();
            builder.AppendLine("        var n = 1;");
            indent();
            builder.AppendLine("        do");
            indent();
            builder.AppendLine("        {");
            indent();
            builder.AppendLine("            var current = enumerator.Current;");
            indent();
            builder.AppendLine("            var state = new State(n == 0, !enumerator.MoveNext(), n % 2 == 0);");
            indent();
            builder.AppendLine("            n++;");
            indent();
            builder.AppendLine("            writeFn(writer, current, state);");
            indent();
            builder.AppendLine("            if (state.Last)");
            indent();
            builder.AppendLine("                break;");
            indent();
            builder.AppendLine("        }");
            indent();
            builder.AppendLine("        while (true);");
            indent();
            builder.AppendLine("    }");
            indent();
            builder.AppendLine("}");

            /*
             *         using (IEnumerator<int> enumerator = numbers.GetEnumerator())
        {
            if (!enumerator.MoveNext())
            {
                Console.WriteLine("The collection is empty.");
                return;
            }

            int current;
            do
            {
                current = enumerator.Current;

                if (enumerator.MoveNext())
                {
                    // Not the last item
                    Console.WriteLine($"Current item is {current}, and it's not the last one.");
                }
                else
                {
                    // Last item
                    Console.WriteLine($"Current item is {current}, and it's the last one.");
                    break;
                }
            }
            while (true);
        }

             */

            var macrosNeedingTypes =
                context.Root.Descendants().Where(m => m.Type == MacroType.ForEach && !m.IsSimpleList())
                    .ToList();

            var generatedIndentation = "";

            void writeIndenter(string newIndentation, bool force)
            {
                if (string.Equals(generatedIndentation, newIndentation))
                    return;
                generatedIndentation = newIndentation;
                if (context.Indentation || force)
                {
                    indent();
                    builder.Append("builder.Indentation = \"").Append(newIndentation).AppendLine("\";");
                }
            }

            void writeText(Macro macro)
            {
                indent();
                if (macro.Source == "\r\n" || macro.Source == "\n")
                    builder.AppendLine("builder.AppendLine();");
                else
                {
                    writeIndenter("", true);
                    builder.Append("builder.Append(@\"")
                        .Append(macro.Source.Replace("\"", "\"\""))
                        .AppendLine("\");");
                }
            }

            void generateWriterFunction(Macro macro)
            {
                indent();
                builder.Append("private void Write").Append(macro.SourceName()).Append(macro.Id).Append("(Writer builder, ").Append(macro.ItemTypeName()).AppendLine(" item, in State state)");
                indent();
                builder.AppendLine("{");
                indentation++;

                void writeContent(Macro parent)
                {
                    foreach (var submacro in parent.Macros)
                    {
                        switch (submacro.Type)
                        {
                            case MacroType.Text:
                                writeText(submacro);
                                break;
                            case MacroType.ForEach:
                                indent();
                                builder.Append("foreach (var item in Get").Append(submacro.SourceName())
                                    .AppendLine("())");
                                indent();
                                builder.AppendLine("{");
                                indentation++;
                                if (submacro.IsSimpleList())
                                    writeContent(submacro);
                                else
                                {
                                    indent();
                                    builder.Append("Write").Append(submacro.SourceName()).Append(submacro.Id)
                                        .AppendLine("(builder, item);");
                                }

                                indentation--;
                                indent();
                                builder.AppendLine("}");
                                break;
                            case MacroType.If:
                                indent();
                                builder.Append("if (");
                                if (submacro.IsNot)
                                    builder.Append("!");
                                if (submacro.IsStateField)
                                    builder.Append("state.").Append(submacro.SourceName()).AppendLine(")");
                                else
                                    builder.Append("item.Get").Append(submacro.SourceName()).AppendLine("())");
                                indent();
                                builder.AppendLine("{");
                                indentation++;
                                writeContent(submacro);
                                indentation--;
                                indent();
                                builder.AppendLine("}");
                                break;
                            case MacroType.ElseIf:
                                indent();
                                if (submacro.IsStateField)
                                    builder.Append("else if (state.").Append(submacro.SourceName()).AppendLine(")");
                                else
                                    builder.Append("if (item.Get").Append(submacro.SourceName()).AppendLine("())");
                                indent();
                                builder.AppendLine("{");
                                indentation++;
                                writeContent(submacro);
                                indentation--;
                                indent();
                                builder.AppendLine("}");
                                break;
                            case MacroType.Else:
                                indent();
                                builder.AppendLine("else");
                                indent();
                                builder.AppendLine("{");
                                indentation++;
                                writeContent(submacro);
                                indentation--;
                                indent();
                                builder.AppendLine("}");
                                break;
                            case MacroType.Ref:
                                writeIndenter(submacro.GenerateIndent(), submacro.ForceIndent());
                                indent();
                                builder.Append("builder.Append(item.Get").Append(submacro.SourceName()).AppendLine("());");
                                writeIndenter("", submacro.ForceIndent());
                                break;
                            case MacroType.Next:
                            case MacroType.End:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                writeContent(macro);

                if (macro.Type == MacroType.Root)
                {
                    indent();
                    builder.AppendLine("return builder.ToString();");
                }

                indentation--;
                indent();
                builder.AppendLine("}");
            }

            void generateClassContent(Macro macro, bool isPublic)
            {
                var generatedMembers = new HashSet<string>();
                foreach (var submacro in macro.Macros)
                {
                    switch (submacro.Type)
                    {
                        case MacroType.ForEach:
                            if (!generatedMembers.Add(submacro.SourceName()))
                                break;
                            indent();
                            if (submacro.IsSimpleList())
                            {
                                builder.Append(isPublic ? "public " : "protected ")
                                    .Append("virtual IEnumerable<string> Get")
                                    .Append(submacro.SourceName()).AppendLine("() { yield break; }");
                            }
                            else
                            {
                                builder.Append(isPublic ? "public " : "protected ").Append("virtual IEnumerable<")
                                    .Append(submacro.ItemTypeName()).Append("> Get")
                                    .Append(submacro.SourceName()).AppendLine("() { yield break; }");
                            }

                            break;
                        case MacroType.Ref:
                            if (submacro.IsStateField || !generatedMembers.Add(submacro.SourceName()))
                                break;
                            indent();
                            builder.Append(isPublic ? "public " : "protected ").Append("virtual string Get")
                                .Append(submacro.SourceName()).AppendLine("() { return string.Empty; }");
                            break;
                        case MacroType.If:
                        case MacroType.ElseIf:
                            if (submacro.IsStateField || !generatedMembers.Add(submacro.SourceName()))
                                break;
                            indent();
                            builder.Append(isPublic ? "public " : "protected ")
                                .Append("virtual bool Get")
                                .Append(submacro.SourceName()).AppendLine("() { return false; }");
                            break;
                        case MacroType.Else:
                        case MacroType.Text:
                        case MacroType.Next:
                        case MacroType.End:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (macro.Type == MacroType.Root)
                {
                    indent();
                    builder.AppendLine("public override string ToString()");
                    indent();
                    builder.AppendLine("{");
                    indentation++;
                    indent();
                    builder.AppendLine("var builder = new Writer();");

                    void writeContent(Macro parent)
                    {
                        foreach (var submacro in parent.Macros)
                        {
                            switch (submacro.Type)
                            {
                                case MacroType.Text:
                                    writeText(submacro);
                                    break;
                                case MacroType.ForEach:
                                    indent();
                                    if (submacro.IsSimpleList())
                                    {
                                        builder.Append("foreach (var item in Get").Append(submacro.SourceName())
                                            .AppendLine("())");
                                        indent();
                                        builder.AppendLine("{");
                                        indentation++;
                                        writeContent(submacro);
                                        indentation--;
                                        indent();
                                        builder.AppendLine("}");
                                    }
                                    else
                                        builder.Append("WriteListItem(Get").Append(submacro.SourceName()).Append("(), builder, ").Append("Write").Append(submacro.SourceName()).Append(submacro.Id).AppendLine(");");

                                    break;
                                case MacroType.If:
                                    indent();
                                    builder.Append("if (");
                                    if (submacro.IsNot)
                                        builder.Append("!");
                                    builder.Append("Get").Append(submacro.SourceName()).AppendLine("())");
                                    indent();
                                    builder.AppendLine("{");
                                    indentation++;
                                    writeContent(submacro);
                                    indentation--;
                                    indent();
                                    builder.AppendLine("}");
                                    break;
                                case MacroType.ElseIf:
                                    indent();
                                    builder.Append("else if (Get").Append(submacro.SourceName()).AppendLine("())");
                                    indent();
                                    builder.AppendLine("{");
                                    indentation++;
                                    writeContent(submacro);
                                    indentation--;
                                    indent();
                                    builder.AppendLine("}");
                                    break;
                                case MacroType.Else:
                                    indent();
                                    builder.AppendLine("else");
                                    indent();
                                    builder.AppendLine("{");
                                    indentation++;
                                    writeContent(submacro);
                                    indentation--;
                                    indent();
                                    builder.AppendLine("}");
                                    break;
                                case MacroType.Ref:
                                    writeIndenter(submacro.GenerateIndent(), submacro.ForceIndent());
                                    indent();
                                    builder.Append("builder.Append(Get").Append(submacro.SourceName())
                                        .AppendLine("());");
                                    writeIndenter("", submacro.ForceIndent());
                                    break;
                                case MacroType.Next:
                                case MacroType.End:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }

                    writeContent(macro);

                    if (macro.Type == MacroType.Root)
                    {
                        indent();
                        builder.AppendLine("return builder.ToString();");
                    }

                    indentation--;
                    indent();
                    builder.AppendLine("}");
                }
            }

            foreach (var macro in macrosNeedingTypes)
                generateWriterFunction(macro);

            Macro merge(IGrouping<string, Macro> macros)
            {
                var macro = new Macro(MacroType.Ref, default, default, "Coalesce", macros.First().Source, false, false, new HashSet<string>());
                foreach (var sub in macros)
                {
                    foreach (var child in sub.Descendants().Where(child => macro.Type != MacroType.ForEach && macro.Macros.All(m => m.SourceName() != child.SourceName())))
                        macro.Macros.Add(child);
                }

                return macro;
            }

            var combinedTypes = macrosNeedingTypes.GroupBy(m => m.ItemTypeName())
                .Select(merge).ToList();

            foreach (var macro in combinedTypes)
            {
                indent();
                builder.Append("public abstract class ").AppendLine(macro.ItemTypeName());
                indent();
                builder.AppendLine("{");
                indentation++;
                generateClassContent(macro, true);

                indentation--;
                indent();
                builder.AppendLine("}");
            }

            generateClassContent(context.Root, false);

            builder.AppendLine("    }");
            builder.AppendLine("}");

            var hintName = Path.GetFileNameWithoutExtension(sourceText.Path) + ".onyx.cs";

            context.AddSource(hintName, builder.ToString());
        }

        static bool TryReadMacro(ref TextPtr ptr, out Macro macro)
        {
            macro = default;
            if (!ptr.StartsWith("{{"))
                return false;
            var end = (ptr + 2).SkipWhitespace(true);
            MacroType type;
            string name = null;
            string source = null;
            var isStateField = false;
            var isStartOfLine = ptr.IsAtStartOfLine();
            var not = false;

            if (end.Char == '$')
            {
                end++;
                if (!TryReadWord(ref end, out var word)) return false;

                if (string.Equals(word, "template", StringComparison.OrdinalIgnoreCase))
                {
                    type = MacroType.Header;
                    name = word;
                }
                else if (string.Equals(word, "foreach", StringComparison.OrdinalIgnoreCase))
                {
                    type = MacroType.ForEach;
                    end = end.SkipWhitespace(true);
                    if (!TryReadWord(ref end, out name)) return false;
                    end = end.SkipWhitespace(true);
                    if (!TryReadWord(ref end, out _, "in")) return false;
                    end = end.SkipWhitespace(true);
                    if (!TryReadWord(ref end, out source)) return false;
                }
                else if (string.Equals(word, "if", StringComparison.OrdinalIgnoreCase))
                {
                    type = MacroType.If;
                    end = end.SkipWhitespace(true);
                    if (TryReadStateName(ref end, out source))
                        isStateField = true;
                    else if (!TryReadWord(ref end, out source))
                        return false;
                    else if (string.Equals(source, "not", StringComparison.OrdinalIgnoreCase))
                    {
                        not = true;
                        end = end.SkipWhitespace(true);
                        if (!TryReadWord(ref end, out source))
                            return false;
                    }
                }
                else if (string.Equals(word, "elseif", StringComparison.OrdinalIgnoreCase))
                {
                    type = MacroType.ElseIf;
                    end = end.SkipWhitespace(true);
                    if (TryReadStateName(ref end, out source))
                        isStateField = true;
                    else if (!TryReadWord(ref end, out source))
                        return false;
                    else if (string.Equals(source, "not", StringComparison.OrdinalIgnoreCase))
                    {
                        not = true;
                        end = end.SkipWhitespace(true);
                        if (!TryReadWord(ref end, out source))
                            return false;
                    }
                }
                else if (string.Equals(word, "else", StringComparison.OrdinalIgnoreCase))
                    type = MacroType.Else;
                else if (string.Equals(word, "next", StringComparison.OrdinalIgnoreCase))
                    type = MacroType.Next;
                else if (string.Equals(word, "end", StringComparison.OrdinalIgnoreCase))
                    type = MacroType.End;
                else
                    return false;
            }
            else
            {
                if (!TryReadWord(ref end, out var word)) return false;
                type = MacroType.Ref;
                source = word;
            }

            end = end.SkipWhitespace(true);

            var wantsTags = false;
            if (type == MacroType.Header)
                wantsTags = true;
            else if (type == MacroType.Ref && end.Char == ':')
            {
                end++;
                end = end.SkipWhitespace(true);
                wantsTags = true;
            }

            HashSet<string> tags = null;
            if (wantsTags)
            {
                tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (true)
                {
                    if (!TryReadWord(ref end, out var tag))
                    {
                        if (tags.Count > 0)
                            break;
                        return false;
                    }

                    tags.Add(tag);
                    end = end.SkipWhitespace(true);
                }
            }

            if (!end.StartsWith("}}")) return false;
            end += 2;
            var isEndOfLine = end.IsNewLine();
            macro = new Macro(type, ptr, end, name, source, isStateField, not, tags);
            if (isStartOfLine && isEndOfLine && type != MacroType.Ref)
                ptr = end.FindEndOfLine(true);
            else
                ptr = end;
            return true;
        }

        static bool TryReadStateName(ref TextPtr ptr, out string macro)
        {
            macro = null;
            if (ptr.Char != '$')
                return false;
            var end = ptr + 1;
            if (!char.IsLetter(end.Char) && end.Char != '_')
                return false;
            end++;
            while (!end.IsPastEnd() && (char.IsLetterOrDigit(end.Char) || end.Char == '_'))
                end++;
            var value = (ptr + 1).TakeUntil(end).ToString();
            if (KnownStates.All(s => !string.Equals(s, value, StringComparison.OrdinalIgnoreCase)))
                return false;
            macro = value;
            ptr = end;
            return true;
        }

        static bool TryReadWord(ref TextPtr ptr, out string word, string keyword = null)
        {
            word = null;
            if (!char.IsLetter(ptr.Char) && ptr.Char != '_')
                return false;
            var end = ptr + 1;
            while (!end.IsPastEnd() && (char.IsLetterOrDigit(end.Char) || end.Char == '_'))
                end++;
            word = ptr.TakeUntil(end).ToString();
            if (keyword != null && !string.Equals(word, keyword, StringComparison.OrdinalIgnoreCase))
                return false;
            ptr = end;
            return true;
        }
    }
}