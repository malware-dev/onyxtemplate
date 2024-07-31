// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Mal.OnyxTemplate.DocumentModel;

namespace Mal.OnyxTemplate
{
    public class DocumentWriter
    {
        static readonly string[] NewLines = { "\r\n", "\n" };

        readonly bool _supportNullable;
        readonly TextWriter _writer;

        public DocumentWriter(TextWriter writer, bool supportNullable)
        {
            _writer = writer;
            _supportNullable = supportNullable;
        }

        public void Write(Document document, string rootNamespace, Identifier className, bool publicAccess)
        {
            var file = new ScopedWriter(_writer, 0);
            file.AppendLineIf(_supportNullable, "#nullable disable")
                .AppendLine("using System;")
                .AppendLine("using System.Text;")
                .AppendLine("using System.Collections.Generic;")
                .AppendLine()
                .AppendLine("#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member.")
                .Append("namespace ").AppendLine(rootNamespace ?? "OnyxTemplates")
                .AppendLine("{").Indent()
                .AppendIf(publicAccess, "public ", "internal ").Append("class ").Append(className.ToString()).AppendLine(": Mal.OnyxTemplate.TextTemplate")
                .AppendLine("{").Indent();
            var typeDescriptor = document.ToTemplateTypeDescriptor();
            WriteProperties(file, typeDescriptor);
            file.AppendLine();
            var scope = new WriteScope(typeDescriptor, document.NeedsMacroState());
            WriteWriteMethod(file, document, scope);
            if (typeDescriptor.ComplexTypes.Length > 0)
            {
                file.AppendLine();
                for (int i = 0, n = typeDescriptor.ComplexTypes.Length - 1; i <= n; i++)
                {
                    var nestedType = typeDescriptor.ComplexTypes[i];
                    WriteType(file, nestedType);

                    if (i < n)
                        file.AppendLine();
                }
            }

            file.Unindent().AppendLine("}")
                .Unindent().AppendLine("}");
        }

        static void WriteWriteMethod(ScopedWriter file, Document document, WriteScope scope)
        {
            file.AppendLine("public override string ToString()")
                .AppendLine("{").Indent()
                .AppendLine("var writer = new Writer();");

            if (scope.UsesMacroState())
                file.AppendLine("State __macro__ = null;");

            WriteWriterBlocks(document, file, document.Blocks, scope);
            file.AppendLine("return writer.ToString();")
                .Unindent().AppendLine("}");
        }

        static void WriteWriterBlocks(Document document, ScopedWriter file, ImmutableArray<DocumentBlock> blocks, WriteScope scope)
        {
            string resolveField(DocumentFieldReference fieldRef)
            {
                if (scope.ItemName.IsDefined() && fieldRef.Up == 0 && fieldRef.Name.EqualsIgnoreCase(scope.ItemName.ToStringSegment()))
                    return scope.ItemNameAlias;

                if (fieldRef.MacroKind != MacroKind.None)
                {
                    var sb = new StringBuilder();
                    sb.Append("__macro__.");
                    for (var i = 0; i < fieldRef.Up; i++)
                        sb.Append("Parent.");
                    sb.Append(fieldRef.Name.Text, fieldRef.Name.Start, fieldRef.Name.Length);
                    return sb.ToString();
                }

                scope.Resolve(fieldRef, out var field, out var origin);
                return origin.ItemNameAlias + "." + field.Name;
            }

            foreach (var block in blocks)
            {
                string fieldReference;
                switch (block)
                {
                    case TextBlock textBlock:
                        var escapedText = textBlock.Text.ToString().Replace("\"", "\"\"");
                        var lines = escapedText.Split(NewLines, StringSplitOptions.None);
                        for (int i = 0, n = lines.Length - 1; i <= n; i++)
                        {
                            var line = lines[i];
                            if (i < n)
                            {
                                if (string.IsNullOrEmpty(line))
                                    file.AppendLine("writer.AppendLine();");
                                else
                                    file.Append("writer.AppendLine(@\"").Append(line).AppendLine("\");");
                            }
                            else if (!string.IsNullOrEmpty(line))
                                file.Append("writer.Append(@\"").Append(line).AppendLine("\");");
                        }

                        break;
                    case SimpleMacroBlock simpleMacro:
                        fieldReference = resolveField(simpleMacro.Field);
                        file.Append("writer.Append(").Append(fieldReference).AppendLineIf(simpleMacro.Indent || document.Header.Indent, ", true);", ");");
                        break;
                    case ConditionalMacro ifBlock:
                        for (var index = 0; index < ifBlock.IfSections.Length; index++)
                        {
                            var ifSection = ifBlock.IfSections[index];
                            fieldReference = resolveField(ifSection.Field);
                            var field = scope.Resolve(ifSection.Field);
                            switch (field.Type)
                            {
                                case TemplateFieldType.Boolean:
                                    break;
                                case TemplateFieldType.String:
                                    fieldReference = $"{fieldReference} != null";
                                    break;
                                case TemplateFieldType.Collection:
                                    fieldReference = $"({fieldReference} != null && {fieldReference}.Any())";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            file.AppendIf(index > 0, "else if (", "if (").AppendIf(ifSection.Not, "!").Append(fieldReference).AppendLine(")")
                                .AppendLine("{").Indent();
                            WriteWriterBlocks(document, file, ifSection.Blocks, scope);
                            file.Unindent().AppendLine("}");
                        }

                        if (ifBlock.ElseSection != null)
                        {
                            file.AppendLine("else")
                                .AppendLine("{").Indent();
                            WriteWriterBlocks(document, file, ifBlock.ElseSection.Blocks, scope);
                            file.Unindent().AppendLine("}");
                        }

                        break;

                    case ForEachMacroBlock forEachBlock:
                        var collectionField = resolveField(forEachBlock.Collection);
                        var collection = scope.Resolve(forEachBlock.Collection);
                        var itemName = Identifier.MakeSafe(forEachBlock.Variable, true);
                        var itemNameAlias = scope.GetVariableName();
                        var usesMacroState = scope.UsesMacroState();
                        if (usesMacroState)
                            file.Append("__macro__ = new State(").Append(collectionField).AppendLine(".Count, __macro__);");

                        file.Append("for (int i = 0, n = ").Append(collectionField).AppendLine(".Count - 1; i <= n; i++)")
                            .AppendLine("{").Indent()
                            .Append("var ").Append(itemNameAlias).Append(" = ").Append(collectionField).Append("[i];").AppendLine();
                        if (usesMacroState)
                            file.AppendLine("__macro__.Index = i;");
                        var innerScope = new WriteScope(scope, collection.ComplexType, itemName, itemNameAlias);
                        WriteWriterBlocks(document, file, forEachBlock.Blocks, innerScope);
                        file.Unindent().AppendLine("}");

                        if (usesMacroState)
                            file.AppendLine("__macro__ = __macro__.Parent;");

                        break;
                }
            }
        }

        static void WriteProperties(ScopedWriter file, TemplateTypeDescriptor typeDescriptor)
        {
            for (int i = 0, n = typeDescriptor.Fields.Length - 1; i <= n; i++)
            {
                var field = typeDescriptor.Fields[i];
                string fieldName = null;
                if (field.Type == TemplateFieldType.Collection)
                {
                    fieldName = field.Name.AsCamelCase().WithPrefix("_").ToString();
                    file.Append("IReadOnlyList<");
                    switch (field.ElementType)
                    {
                        case TemplateFieldType.Boolean:
                            file.Append("bool");
                            break;
                        case TemplateFieldType.String:
                            file.Append("string");
                            break;
                        case TemplateFieldType.Complex:
                            file.Append(field.ComplexType.Name.ToString());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    file.Append("> ").Append(fieldName).AppendLine(";");
                }

                file.Append("public virtual ");
                switch (field.Type)
                {
                    case TemplateFieldType.Boolean:
                        file.Append("bool").Append(" ").Append(field.Name.ToString()).AppendLine(" { get; set; }");
                        break;
                    case TemplateFieldType.String:
                        file.Append("string").Append(" ").Append(field.Name.ToString()).AppendLine(" { get; set; }");
                        break;
                    case TemplateFieldType.Complex:
                        file.Append(field.ComplexType.Name.ToString()).Append(" ").Append(field.Name.ToString()).AppendLine(" { get; set; }");
                        break;
                    case TemplateFieldType.Collection:
                        file.Append("IReadOnlyList<");
                        string elementTypeName;
                        switch (field.ElementType)
                        {
                            case TemplateFieldType.Boolean:
                                elementTypeName = "bool";
                                break;
                            case TemplateFieldType.String:
                                elementTypeName = "string";
                                break;
                            case TemplateFieldType.Complex:
                                elementTypeName = field.ComplexType.Name.ToString();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        file.Append(elementTypeName).Append(">").Append(" ").Append(field.Name.ToString());
                        if (field.Type == TemplateFieldType.Collection)
                            file.Append(" { get { return ").Append(fieldName).Append(" ?? Array.Empty<").Append(elementTypeName).Append(">(); } set { ").Append(fieldName).AppendLine(" = value; } }");
                        else
                            file.AppendLine(" { get; set; }");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        static void WriteType(ScopedWriter file, TemplateTypeDescriptor nestedType)
        {
            file.Append("public class ").AppendLine(nestedType.Name.ToString())
                .AppendLine("{").Indent();
            WriteProperties(file, nestedType);
            file.Unindent().AppendLine("}");
        }

        class WriteScope
        {
            readonly bool _usesMacroState;
            uint _variableCounter;

            public WriteScope(TemplateTypeDescriptor typeDescriptor, bool usesMacroState)
            {
                TypeDescriptor = typeDescriptor;
                _usesMacroState = usesMacroState;
            }

            public WriteScope(WriteScope parent, TemplateTypeDescriptor typeDescriptor, Identifier itemName, string itemNameAlias)
            {
                TypeDescriptor = typeDescriptor;
                ItemName = itemName;
                ItemNameAlias = itemNameAlias;
                Parent = parent;
            }

            TemplateTypeDescriptor TypeDescriptor { get; }
            public Identifier ItemName { get; }
            public string ItemNameAlias { get; } = "this";
            public WriteScope Parent { get; }
            public bool UsesMacroState() => Parent?.UsesMacroState() ?? _usesMacroState;

            public string GetVariableName()
            {
                if (Parent != null)
                    return Parent.GetVariableName();
                return "v" + ++_variableCounter;
            }

            public TemplateFieldDescriptor Resolve(DocumentFieldReference field)
            {
                Resolve(field, out var descriptor, out _);
                return descriptor;
            }
            
            public void Resolve(DocumentFieldReference field, out TemplateFieldDescriptor descriptor, out WriteScope scope)
            {
                var parent = this;
                for (var i = 0; i < field.Up; i++)
                {
                    parent = parent.Parent;
                    if (parent == null)
                        throw new DomException(field.Source, field.Name.Length, "Cannot resolve field reference: Too many levels up.");
                }

                switch (field.MacroKind)
                {
                    case MacroKind.None:
                        descriptor = parent.TypeDescriptor.Fields.FirstOrDefault(f => field.Name.EqualsIgnoreCase(f.Name.ToStringSegment()));
                        if (descriptor == null)
                            throw new DomException(field.Source, field.Name.Length, $"Cannot resolve field reference: {field.Name}.");
                        scope = parent;
                        return;
                    case MacroKind.First:
                        descriptor = Document.Macros.First;
                        scope = parent;
                        return;
                    case MacroKind.Last:
                        descriptor = Document.Macros.Last;
                        scope = parent;
                        return;
                    case MacroKind.Middle:
                        descriptor = Document.Macros.Middle;
                        scope = parent;
                        return;
                    case MacroKind.Odd:
                        descriptor = Document.Macros.Odd;
                        scope = parent;
                        return;
                    case MacroKind.Even:
                        descriptor = Document.Macros.Even;
                        scope = parent;
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}