// OnyxTemplate.Tests
// 
// Copyright 2024 Morten Aune Lyrstad

using FluentAssertions;
using Mal.OnyxTemplate.DocumentModel;
using NUnit.Framework;

namespace OnyxTemplate.Tests;

[TestFixture]
public class ForEachMacroTests
{
    [Test]
    public void Parse_WithForEachOnSeparateLines_ShouldParseCorrectly()
    {
        var template =
            """
            {{ $foreach item in items }}
                {{ item }}
            {{ $next }}
            """;

        var document = Document.Parse(template);
        document.Blocks.Length.Should().Be(1);
        var block = document.Blocks[0].Should().BeOfType<ForEachMacroBlock>().Subject;
        block.Variable.ToString().Should().Be("item");
        block.Collection.Up.Should().Be(0);
        block.Collection.Name.ToString().Should().Be("items");
        block.Blocks.Length.Should().Be(1);
        block.Blocks[0].Should().BeOfType<SimpleMacroBlock>().Which.Field.Name.ToString().Should().Be("item");
    }
    
    [Test]
    public void Parse_WithForEachOnSameLine_ShouldParseCorrectly()
    {
        var template = "{{ $foreach item in items }}{{ item }}{{ $next }}";

        var document = Document.Parse(template);
        document.Blocks.Length.Should().Be(1);
        var block = document.Blocks[0].Should().BeOfType<ForEachMacroBlock>().Subject;
        block.Variable.ToString().Should().Be("item");
        block.Collection.Up.Should().Be(0);
        block.Collection.Name.ToString().Should().Be("items");
        block.Blocks.Length.Should().Be(1);
        block.Blocks[0].Should().BeOfType<SimpleMacroBlock>().Which.Field.Name.ToString().Should().Be("item");
    }
}