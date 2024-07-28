// OnyxTemplate.Tests
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

                    public string ToString()
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
        documentWriter.Write(document, "OnyxTemplates", "MyTemplate", true);
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

                    public string ToString()
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
        documentWriter.Write(document, "OnyxTemplates", "MyTemplate", true);
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

                    public string ToString()
                    {
                        var writer = new Writer();
                        State __macro__ = null;
                        __macro__ = new State(__macro__);
                        for (int i = 0, n = this.Items.Count - 1; i <= n; i++)
                        {
                            var v1 = this.Items[i];
                            __macro__.Index = i;
                            writer.Append(@"    ");
                            writer.Append(v1.item);
                            writer.Append(@" ");
                            writer.Append(v1.item);
                            writer.AppendLine();
                        }
                        __macro__ = __macro__.Parent;
                        return writer.ToString();
                    }
            
            """ +
            Suffix;

        var document = Document.Parse(template);
        var writer = new StringWriter();
        var documentWriter = new DocumentWriter(writer, false);
        documentWriter.Write(document, "OnyxTemplates", "MyTemplate", true);
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
                    IReadOnlyList<ItemItem> _items;
                    public virtual IReadOnlyList<ItemItem> Items { get { return _items ?? Array.Empty<ItemItem>(); } set { _items = value; } }

                    public string ToString()
                    {
                        var writer = new Writer();
                        State __macro__ = null;
                        __macro__ = new State(__macro__);
                        for (int i = 0, n = this.Items.Count - 1; i <= n; i++)
                        {
                            var v1 = this.Items[i];
                            __macro__.Index = i;
                            writer.Append(v1.item);
                            writer.Append(v1.item);
                            writer.AppendLine();
                            __macro__ = new State(__macro__);
                            for (int i = 0, n = v1.SubItems.Count - 1; i <= n; i++)
                            {
                                var v2 = v1.SubItems[i];
                                __macro__.Index = i;
                                writer.Append(v2.FirstName);
                                writer.Append(v2.LastName);
                                writer.AppendLine();
                            }
                            __macro__ = __macro__.Parent;
                        }
                        __macro__ = __macro__.Parent;
                        return writer.ToString();
                    }

                    public class ItemItem
                    {
                        public virtual string Item { get; set; }
                        IReadOnlyList<SubItemItem> _subItems;
                        public virtual IReadOnlyList<SubItemItem> SubItems { get { return _subItems ?? Array.Empty<SubItemItem>(); } set { _subItems = value; } }
                    }

                    public class SubItemItem
                    {
                        public virtual string FirstName { get; set; }
                        public virtual string LastName { get; set; }
                    }

            """ +
            Suffix;

        var document = Document.Parse(template);
        var writer = new StringWriter();
        var documentWriter = new DocumentWriter(writer, false);
        documentWriter.Write(document, "OnyxTemplates", "MyTemplate", true);
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

                    public string ToString()
                    {
                        var writer = new Writer();
                        writer.Append(@"The following text is unindented: ");
                        writer.Append(this.Unindented);
                        writer.AppendLine();
                        writer.Append(@"The following text is indented: ");
                        writer.Append(Indent(writer.Col, this.Indented));
                        return writer.ToString();
                    }

            """ +
            Suffix;

        var document = Document.Parse(template);
        var writer = new StringWriter();
        var documentWriter = new DocumentWriter(writer, false);
        documentWriter.Write(document, "OnyxTemplates", "MyTemplate", true);
        var result = writer.ToString();
        result.Should().Be(expected);
    }
}