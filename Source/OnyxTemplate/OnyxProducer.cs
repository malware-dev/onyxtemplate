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
    /// Generates the source code for the runtime template of a single .onyx file.
    /// </summary>
    class OnyxProducer
    {
        /// <summary>
        /// Generates the source code for the runtime template of a single .onyx file.
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
                    context.Add(new Macro(MacroType.Text, start, end, null, text, null));
                }

                start = end;
            }

            while (!end.IsPastEnd())
            {
                var after = end;
                if (TryReadMacro(ref after, out var macro))
                {
                    if (macro.Type == MacroType.Next && !context.IsSubScope)
                        continue;
                    pushText();
                    if (macro.Type == MacroType.Next)
                        context.PopScope();
                    start = end = after;
                    context.Add(macro);
                    continue;
                }

                end++;
            }

            pushText();

            var builder = new StringBuilder();
            builder.AppendLine("#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.");
            builder.AppendLine("using System.Text;");
            builder.AppendLine();
            builder.Append("namespace ").AppendLine(context.RootNamespace ?? "OnyxTemplates");
            builder.AppendLine("{");
            builder.Append("    ").Append(context.PublicClass ? "public " : "internal ").Append("abstract class ").AppendLine(className);
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
                    builder.Append("builder.Indentation = \"").Append(newIndentation).AppendLine("\";");
            }

            void writeText(Macro macro)
            {
                indent();
                if (macro.Source == "\r\n" || macro.Source == "\n")
                    builder.AppendLine("builder.AppendLine();");
                else
                {
                    writeIndenter("", true);
                    // indent();
                    builder.Append("builder.Append(@\"")
                        .Append(macro.Source.Replace("\"", "\"\""))
                        .AppendLine("\");");
                    // indent();
                }
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
                            if (!generatedMembers.Add(submacro.SourceName()))
                                break;
                            indent();
                            builder.Append(isPublic ? "public " : "protected ").Append("virtual string Get")
                                .Append(submacro.SourceName()).AppendLine("() { return string.Empty; }");
                            break;
                        case MacroType.Text:
                        case MacroType.Next:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                indent();
                builder.AppendLine("public override string ToString()");
                indent();
                builder.AppendLine("{");
                indentation++;
                indent();
                builder.AppendLine("var builder = new Writer();");


                foreach (var submacro in macro.Macros)
                {
                    switch (submacro.Type)
                    {
                        case MacroType.Text:
                            writeText(submacro);
                            break;
                        case MacroType.ForEach:
                            indent();
                            builder.Append("foreach (var item in Get").Append(submacro.SourceName()).AppendLine("())");
                            indent();
                            builder.AppendLine("{");
                            indentation++;
                            if (submacro.IsSimpleList())
                            {
                                foreach (var simplemacro in submacro.Macros)
                                {
                                    switch (simplemacro.Type)
                                    {
                                        case MacroType.Text:
                                            writeText(submacro);
                                            break;
                                        case MacroType.Ref:
                                            writeIndenter(simplemacro.GenerateIndent(), simplemacro.ForceIndent());
                                            indent();
                                            builder.AppendLine("builder.Append(item);");
                                            indent();
                                            writeIndenter("", simplemacro.ForceIndent());
                                            break;
                                        case MacroType.Next:
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                            }
                            else
                            {
                                indent();
                                builder.AppendLine("builder.Append(item.ToString());");
                            }

                            indentation--;
                            indent();
                            builder.AppendLine("}");
                            break;
                        case MacroType.Ref:
                            indent();
                            writeIndenter(submacro.GenerateIndent(), submacro.ForceIndent());
                            indent();
                            builder.Append("builder.Append(Get").Append(submacro.SourceName()).AppendLine("());");
                            indent();
                            writeIndenter("", submacro.ForceIndent());
                            break;
                        case MacroType.Next:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                indent();
                builder.AppendLine("return builder.ToString();");
                indentation--;
                indent();
                builder.AppendLine("}");
            }

            foreach (var macro in macrosNeedingTypes)
            {
                indent();
                builder.Append("protected abstract class ").AppendLine(macro.ItemTypeName());
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
            var isStartOfLine = ptr.IsAtStartOfLine();

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
                else if (string.Equals(word, "next", StringComparison.OrdinalIgnoreCase))
                    type = MacroType.Next;
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
            else if (end.Char == ':')
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
            macro = new Macro(type, ptr, end, name, source, tags);
            if (isStartOfLine && isEndOfLine && type != MacroType.Ref)
                ptr = end.FindEndOfLine(true);
            else
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