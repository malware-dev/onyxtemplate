// OnyxTemplate.Tests
// 
// Copyright 2024 Morten Aune Lyrstad

using FluentAssertions;
using Mal.OnyxTemplate.DocumentModel;
using NUnit.Framework;

namespace OnyxTemplate.Tests;

[TestFixture]
public class DocumentSimpleMacroTests
{
    [Test]
    public void Parse_WithMixOfTextAndMacros_ShouldParseSuccessfully()
    {
        var template =
            """
            {{ $template public }}
            Hello, {{ name }}!
            """;
        var document = Document.Parse(template);
        document.Header.IsEmpty().Should().BeFalse();
        document.Blocks.Should().HaveCount(3);
        document.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Hello, ");
        document.Blocks[1].Should().BeOfType<SimpleMacroBlock>().Which.Field.Name.ToString().Should().Be("name");
        document.Blocks[2].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("!");
    }

    [Test]
    public void Parse_WithTaggedMacro_ShouldParseSuccessfully()
    {
        var template =
            """
            hello, {{ name:indented }}!
            """;
        var document = Document.Parse(template);
        document.Header.IsEmpty().Should().BeTrue();
        document.Blocks.Should().HaveCount(3);
        document.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("hello, ");
        var block1 = document.Blocks[1].Should().BeOfType<SimpleMacroBlock>().Subject;
        block1.Field.Name.ToString().Should().Be("name");
        block1.Indent.Should().BeTrue();
        document.Blocks[2].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("!");
    }
    
    [Test]
    public void Parse_WithUpDots_ShouldParseSuccessfully()
    {
        var template =
            """
            hello, {{ .name }}!
            bonjour, {{ ..name }}?
            """;
        var document = Document.Parse(template);
        document.Header.IsEmpty().Should().BeTrue();
        document.Blocks.Should().HaveCount(5);
        document.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("hello, ");
        var block1 = document.Blocks[1].Should().BeOfType<SimpleMacroBlock>().Subject;
        block1.Field.Name.ToString().Should().Be("name");
        block1.Field.Up.Should().Be(1);
        document.Blocks[2].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("!\r\nbonjour, ");
        var block2 = document.Blocks[3].Should().BeOfType<SimpleMacroBlock>().Subject;
        block2.Field.Name.ToString().Should().Be("name");
        block2.Field.Up.Should().Be(2);
        document.Blocks[4].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("?");
    }
}