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

        public void Write(Document document, string rootNamespace, string className, bool publicAccess)
        {
            //var description = document.Header.Description ?? "A template class for generating text.";
            var file = new ScopedWriter(_writer, 0);
            file.AppendLineIf(_supportNullable, "#nullable disable")
                .AppendLine("using System;")
                .AppendLine("using System.Text;")
                .AppendLine("using System.Collections.Generic;")
                .AppendLine()
                .Append("namespace ").AppendLine(rootNamespace ?? "OnyxTemplates")
                .AppendLine("{").Indent()
                //.AppendLine("/// <summary>")
                //.Append("///     ").Append(description).AppendLine()
                //.AppendLine("/// </summary>")
                .AppendIf(publicAccess, "public ", "internal ").Append("class ").Append(className).AppendLine(": Mal.OnyxTemplate.TextTemplate")
                .AppendLine("{").Indent();
            var typeDescriptor = document.ToTemplateTypeDescriptor();
            WriteProperties(file, typeDescriptor);
            file.AppendLine();
            var scope = new WriteScope(typeDescriptor);
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
            file //.AppendLine("/// <summary>")
                //.AppendLine("///     Realizes the template, filling out all fields with the values provided.")
                //.AppendLine("/// </summary>")
                .AppendLine("public override string ToString()")
                .AppendLine("{").Indent()
                .AppendLine("var writer = new Writer();")
                .AppendLine("State __macro__ = null;");

            WriteWriterBlocks(file, document.Blocks, scope);
            file.AppendLine("return writer.ToString();")
                .Unindent().AppendLine("}");
        }

        static void WriteWriterBlocks(ScopedWriter file, ImmutableArray<DocumentBlock> blocks, WriteScope scope)
        {
            string resolveField(DocumentFieldReference field)
            {
                if (scope.ItemName != null)
                {
                    if (field.Up == 0 && field.Name.EqualsIgnoreCase(new StringSegment(scope.ItemName)))
                        return scope.ItemNameAlias;
                    return scope.ItemName + "." + ToFieldExpression(field);
                }

                return "this." + ToFieldExpression(field);
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
                        file.Append("writer.Append(").AppendIf(simpleMacro.Indent, $"Indent(writer.Col, {fieldReference})", fieldReference).AppendLine(");");
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
                            WriteWriterBlocks(file, ifSection.Blocks, scope);
                            file.Unindent().AppendLine("}");
                        }

                        if (ifBlock.ElseSection != null)
                        {
                            file.AppendLine("else")
                                .AppendLine("{").Indent();
                            WriteWriterBlocks(file, ifBlock.ElseSection.Blocks, scope);
                            file.Unindent().AppendLine("}");
                        }

                        break;

                    case ForEachMacroBlock forEachBlock:
                        var collectionField = resolveField(forEachBlock.Collection);
                        var collection = scope.Resolve(forEachBlock.Collection);
                        var itemName = Document.CSharpify(forEachBlock.Variable.ToString(), true);
                        var itemNameAlias = scope.GetVariableName();
                        file.Append("__macro__ = new State(").Append(collectionField).AppendLine(".Count, __macro__);")
                            .Append("for (int i = 0, n = ").Append(collectionField).AppendLine(".Count - 1; i <= n; i++)")
                            .AppendLine("{").Indent()
                            .Append("var ").Append(itemNameAlias).Append(" = ").Append(collectionField).Append("[i];").AppendLine()
                            .AppendLine("__macro__.Index = i;");
                        var innerScope = new WriteScope(scope, collection.ComplexType, itemName, itemNameAlias);
                        WriteWriterBlocks(file, forEachBlock.Blocks, innerScope);
                        file.Unindent().AppendLine("}")
                            .AppendLine("__macro__ = __macro__.Parent;");


                        break;
                }
            }
        }

        static string ToFieldExpression(DocumentFieldReference field)
        {
            var sb = new StringBuilder();
            if (field.IsMacroReference)
                sb.Append("__macro__.");
            for (var i = 0; i < field.Up; i++)
                sb.Append("Parent.");
            sb.Append(field.Name.ToString());
            return sb.ToString();
        }

        static void WriteProperties(ScopedWriter file, TemplateTypeDescriptor typeDescriptor)
        {
            for (int i = 0, n = typeDescriptor.Fields.Length - 1; i <= n; i++)
            {
                var field = typeDescriptor.Fields[i];
                string fieldName = null;
                if (field.Type == TemplateFieldType.Collection)
                {
                    fieldName = "_" + Document.CSharpify(field.Name, true);
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
                            file.Append(field.ComplexType.Name);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    file.Append("> ").Append(fieldName).AppendLine(";");
                }

                //file.AppendLine("/// <summary>")
                //    .Append("///     Set the value of the {{ ").Append(field.Name).AppendLine(" }} field.")
                //    .AppendLine("/// </summary>");
                file.Append("public virtual ");
                switch (field.Type)
                {
                    case TemplateFieldType.Boolean:
                        file.Append("bool").Append(" ").Append(field.Name).AppendLine(" { get; set; }");
                        break;
                    case TemplateFieldType.String:
                        file.Append("string").Append(" ").Append(field.Name).AppendLine(" { get; set; }");
                        break;
                    case TemplateFieldType.Complex:
                        file.Append(field.ComplexType.Name).Append(" ").Append(field.Name).AppendLine(" { get; set; }");
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
                                elementTypeName = field.ComplexType.Name;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        file.Append(elementTypeName).Append(">").Append(" ").Append(field.Name);
                        if (field.Type == TemplateFieldType.Collection)
                            file.Append(" { get { return ").Append(fieldName).Append(" ?? Array.Empty<").Append(elementTypeName).Append(">(); } set { ").Append(fieldName).AppendLine(" = value; } }");
                        else
                            file.AppendLine(" { get; set; }");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // if (i < n)
                //     file.AppendLine();
            }
        }

        static void WriteType(ScopedWriter file, TemplateTypeDescriptor nestedType)
        {
            file.Append("public class ").AppendLine(nestedType.Name)
                .AppendLine("{").Indent();
            WriteProperties(file, nestedType);
            file.Unindent().AppendLine("}");
        }

        class WriteScope
        {
            uint _variableCounter;

            public WriteScope(TemplateTypeDescriptor typeDescriptor)
            {
                TypeDescriptor = typeDescriptor;
            }

            public WriteScope(WriteScope parent, TemplateTypeDescriptor typeDescriptor, string itemName, string itemNameAlias)
            {
                TypeDescriptor = typeDescriptor;
                ItemName = itemName;
                ItemNameAlias = itemNameAlias;
                Parent = parent;
            }

            TemplateTypeDescriptor TypeDescriptor { get; }
            public string ItemName { get; }
            public string ItemNameAlias { get; }
            WriteScope Parent { get; }

            public string GetVariableName()
            {
                if (Parent != null)
                    return Parent.GetVariableName();
                return "v" + ++_variableCounter;
            }

            public TemplateFieldDescriptor Resolve(DocumentFieldReference field)
            {
                var parent = this;
                for (var i = 0; i < field.Up; i++)
                {
                    parent = parent.Parent;
                    if (parent == null)
                        throw new DomException(field.Source, field.Name.Length, "Cannot resolve field reference: Too many levels up.");
                }

                var descriptor = parent.TypeDescriptor.Fields.FirstOrDefault(f => field.Name.EqualsIgnoreCase(new StringSegment(f.Name)));
                if (descriptor == null)
                    throw new DomException(field.Source, field.Name.Length,  $"Cannot resolve field reference: {field.Name}.");
                return descriptor;
            }
        }
    }
}