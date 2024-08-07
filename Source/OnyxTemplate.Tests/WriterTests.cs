﻿// OnyxTemplate.Tests
// 
// Copyright 2024 Morten Aune Lyrstad

using FluentAssertions;
using Mal.OnyxTemplate;
using Mal.OnyxTemplate.DocumentModel;
using NUnit.Framework;

namespace OnyxTemplate.Tests;

[TestFixture]
public class WriterTests
{
    const string Prefix =
        """
        using System;
        using System.Text;
        using System.Collections.Generic;

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member.
        namespace OnyxTemplates
        {
            public class MyTemplate: Mal.OnyxTemplate.TextTemplate
            {

        """;

    const string Suffix =
        """
            }
        }

        """;

    [Test]
    public void Write_WithSimpleDocument_ShouldWriteDocument()
    {
        const string template =
            """
            Hello, {{ UserName }}, welcome to the show.
            """;

        const string expected =
            Prefix +
            """
                    public virtual string UserName { get; set; }

                    public override string ToString()
                    {
                        var writer = new Writer();
                        writer.Append(@"Hello, ");
                        writer.Append(this.UserName);
                        writer.Append(@", welcome to the show.");
                        return writer.ToString();
                    }

            """ +
            Suffix;

        var document = Document.Parse(template);
        var writer = new StringWriter();
        var documentWriter = new DocumentWriter(writer, false);
        documentWriter.Write(document, "OnyxTemplates", new Identifier("MyTemplate"), true);
        var result = writer.ToString();
        result.Should().Be(expected);
    }
    
    [Test]
    public void Write_WithConditionalDocument_ShouldWriteDocument()
    {
        const string template =
            """
            {{ $if IsAdmin }}
            Hello, {{ UserName }}, you have admin rights.
            {{ $else }}
            Hello, {{ UserName }}, welcome to the show.
            {{ $end }}
            {{ $if WorkTitle }}
            Your work title is {{ WorkTitle }}.
            {{ $end }}
            """;

        const string expected =
            Prefix +
            """
                    public virtual bool IsAdmin { get; set; }
                    public virtual string UserName { get; set; }
                    public virtual string WorkTitle { get; set; }

                    public override string ToString()
                    {
                        var writer = new Writer();
                        if (this.IsAdmin)
                        {
                            writer.Append(@"Hello, ");
                            writer.Append(this.UserName);
                            writer.AppendLine(@", you have admin rights.");
                        }
                        else
                        {
                            writer.Append(@"Hello, ");
                            writer.Append(this.UserName);
                            writer.AppendLine(@", welcome to the show.");
                        }
                        if (this.WorkTitle != null)
                        {
                            writer.Append(@"Your work title is ");
                            writer.Append(this.WorkTitle);
                            writer.AppendLine(@".");
                        }
                        return writer.ToString();
                    }

            """ +
            Suffix;

        var document = Document.Parse(template);
        var writer = new StringWriter();
        var documentWriter = new DocumentWriter(writer, false);
        documentWriter.Write(document, "OnyxTemplates", new Identifier("MyTemplate"), true);
        var result = writer.ToString();
        result.Should().Be(expected);
    }
    
    [Test]
    public void Write_WithForEachDocument_ShouldWriteDocument()
    {
        const string template =
            """
            {{ $foreach item in Items }}
                {{ item }} {{ item }}
            {{ $next }}
            """;

        const string expected =
            Prefix +
            """
                    IReadOnlyList<string> _items;
                    public virtual IReadOnlyList<string> Items { get { return _items ?? Array.Empty<string>(); } set { _items = value; } }

                    public override string ToString()
                    {
                        var writer = new Writer();
                        for (int v1 = 0, v2 = this.Items.Count - 1; v1 <= v2; v1++)
                        {
                            var v3 = this.Items[v1];
                            writer.Append(@"    ");
                            writer.Append(v3);
                            writer.Append(@" ");
                            writer.Append(v3);
                            writer.AppendLine();
                        }
                        return writer.ToString();
                    }
            
            """ +
            Suffix;

        var document = Document.Parse(template);
        var writer = new StringWriter();
        var documentWriter = new DocumentWriter(writer, false);
        documentWriter.Write(document, "OnyxTemplates", new Identifier("MyTemplate"), true);
        var result = writer.ToString();
        result.Should().Be(expected);
    }
    
    [Test]
    public void Write_WithNestedForEachDocument_ShouldWriteDocument()
    {
        const string template =
            """
            {{ $foreach item in Items }}
            {{ item }} {{ item }}
            {{ $foreach subItem in SubItems }}
            {{ FirstName }} {{ LastName }}
            {{ $next }}
            {{ $next }}
            """;

        const string expected =
            Prefix +
            """
                    IReadOnlyList<ItemsItem> _items;
                    public virtual IReadOnlyList<ItemsItem> Items { get { return _items ?? Array.Empty<ItemsItem>(); } set { _items = value; } }

                    public override string ToString()
                    {
                        var writer = new Writer();
                        for (int v1 = 0, v2 = this.Items.Count - 1; v1 <= v2; v1++)
                        {
                            var v3 = this.Items[v1];
                            writer.Append(v3);
                            writer.Append(v3);
                            writer.AppendLine();
                            for (int v4 = 0, v5 = v3.SubItems.Count - 1; v4 <= v5; v4++)
                            {
                                var v6 = v3.SubItems[v4];
                                writer.Append(v6.FirstName);
                                writer.Append(v6.LastName);
                                writer.AppendLine();
                            }
                        }
                        return writer.ToString();
                    }

                    public class ItemsItem
                    {
                        public virtual string Item { get; set; }
                        IReadOnlyList<SubItemsItem> _subItems;
                        public virtual IReadOnlyList<SubItemsItem> SubItems { get { return _subItems ?? Array.Empty<SubItemsItem>(); } set { _subItems = value; } }
                    }

                    public class SubItemsItem
                    {
                        public virtual string FirstName { get; set; }
                        public virtual string LastName { get; set; }
                    }

            """ +
            Suffix;

        var document = Document.Parse(template);
        var writer = new StringWriter();
        var documentWriter = new DocumentWriter(writer, false);
        documentWriter.Write(document, "OnyxTemplates", new Identifier("MyTemplate"), true);
        var result = writer.ToString();
        result.Should().Be(expected);
    }
    
    [Test]
    public void Write_WithIndentation_ShouldWriteDocument()
    {
        const string template =
            """
            The following text is unindented: {{ Unindented }}
            The following text is indented: {{ Indented:indent }}
            """;

        const string expected =
            Prefix +
            """
                    public virtual string Indented { get; set; }
                    public virtual string Unindented { get; set; }

                    public override string ToString()
                    {
                        var writer = new Writer();
                        writer.Append(@"The following text is unindented: ");
                        writer.Append(this.Unindented);
                        writer.AppendLine();
                        writer.Append(@"The following text is indented: ");
                        writer.Append(this.Indented, true);
                        return writer.ToString();
                    }

            """ +
            Suffix;

        var document = Document.Parse(template);
        var writer = new StringWriter();
        var documentWriter = new DocumentWriter(writer, false);
        documentWriter.Write(document, "OnyxTemplates", new Identifier("MyTemplate"), true);
        var result = writer.ToString();
        result.Should().Be(expected);
    }
}