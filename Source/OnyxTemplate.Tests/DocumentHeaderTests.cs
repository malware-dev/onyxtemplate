using FluentAssertions;
using Mal.OnyxTemplate.DocumentModel;
using NUnit.Framework;

namespace OnyxTemplate.Tests;

[TestFixture]
public class DocumentHeaderTests
{
    [TestCase("{{ $template }}", "", "")]
    [TestCase("  \t{{ $template }}", "  \t", "")]
    [TestCase("{{ $template }}  \t", "", "  \t")]
    [TestCase("{{ $template }}\r\n", "", "")]
    [TestCase("  \t{{ $template }}\r\n", "  \t", "")]
    [TestCase("{{ $template }}  \t\r\n", "", "  \t")]
    public void Parse_WithOrWithoutLeadingAndTrailingTrivia_ParsesSuccessfully(string template, string leading, string trailing)
    {
        var document = Document.Parse(template);
        document.Header.IsEmpty().Should().BeFalse();
    }

    [TestCase("{{ $template public }}", true, false)]
    [TestCase("{{ $template indent }}", false, true)]
    [TestCase("{{ $template public indent }}", true, true)]
    [TestCase("{{ $template indent public}}", true, true)]
    public void Parse_WithHeaderFlagsInAnyOrder_ShouldSuccessfullyParse(string template, bool publicVisibility, bool indent)
    {
        var document = Document.Parse(template);
        document.Header.IsEmpty().Should().BeFalse();
        document.Header.PublicVisibility.Should().Be(publicVisibility);
        document.Header.Indent.Should().Be(indent);
    }

    [Test]
    public void Parse_WithContentOnSameLineAsHeader_ShouldThrow()
    {
        var template = "{{ $template }} Hello, World!";
        Action act = () => Document.Parse(template);
        act.Should().Throw<DomException>();
    }

    [Test]
    public void Parse_WithoutHeaderAndNoMacros_ShouldContainTextBlock()
    {
        var template = "Hello, World!";
        var document = Document.Parse(template);
        document.Header.IsEmpty().Should().BeTrue();
        document.Blocks.Should().HaveCount(1);
        document.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.Should().Be(template);
    }

    [Test]
    public void Parse_WithHeaderAndNoMacros_ShouldContainTextBlockWithoutHeader()
    {
        var template =
            """
            {{ $template public }}
            Hello, World!
            """;
        var document = Document.Parse(template);
        document.Header.IsEmpty().Should().BeFalse();
        document.Header.PublicVisibility.Should().BeTrue();
        document.Blocks.Should().HaveCount(1);
        document.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.Should().Be("Hello, World!");
    }
}