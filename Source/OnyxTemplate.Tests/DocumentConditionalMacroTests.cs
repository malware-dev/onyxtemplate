// OnyxTemplate.Tests
// 
// Copyright 2024 Morten Aune Lyrstad

using FluentAssertions;
using Mal.OnyxTemplate.DocumentModel;
using NUnit.Framework;

namespace OnyxTemplate.Tests;

[TestFixture]
public class DocumentConditionalMacroTests
{
    [Test]
    public void Parse_WithConditionalMacroOnSeparateLines_ShouldParseSuccessfully()
    {
        var template =
            """
            {{ $if condition }}
            Hello, World!
            {{ $end }}
            """;
        var document = Document.Parse(template);
        document.Header.IsEmpty().Should().BeTrue();
        document.Blocks.Should().HaveCount(1);
        var block0 = document.Blocks[0].Should().BeOfType<ConditionalMacro>().Subject;
        block0.IfSections.Should().HaveCount(1);
        block0.ElseSection.Should().BeNull();
        var ifSection = block0.IfSections[0].Should().BeOfType<IfMacroSection>().Subject;
        ifSection.Field.Name.ToString().Should().Be("condition");
        ifSection.Blocks.Should().HaveCount(1);
        ifSection.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Hello, World!\r\n");
    }
    
    [Test]
    public void Parse_WithConditionalMacroOnSameLine_ShouldParseSuccessfully()
    {
        var template = "{{ $if condition }}Hello, World!{{ $end }}";
        var document = Document.Parse(template);
        document.Header.IsEmpty().Should().BeTrue();
        document.Blocks.Should().HaveCount(1);
        var block0 = document.Blocks[0].Should().BeOfType<ConditionalMacro>().Subject;
        block0.IfSections.Should().HaveCount(1);
        block0.ElseSection.Should().BeNull();
        var ifSection = block0.IfSections[0].Should().BeOfType<IfMacroSection>().Subject;
        ifSection.Field.Name.ToString().Should().Be("condition");
        ifSection.Blocks.Should().HaveCount(1);
        ifSection.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Hello, World!");
    }
    
     [Test]
     public void Parse_WithIfElseOnSeparateLines_ShouldParseSuccessfully()
     {
         var template =
             """
             {{ $if condition }}
             Hello, World!
             {{ $else }}
             Goodbye, World!
             {{ $end }}
             """;
         var document = Document.Parse(template);
         document.Header.IsEmpty().Should().BeTrue();
         document.Blocks.Should().HaveCount(1);
         var block0 = document.Blocks[0].Should().BeOfType<ConditionalMacro>().Subject;
         block0.IfSections.Should().HaveCount(1);
         block0.ElseSection.Should().NotBeNull();
         var ifSection = block0.IfSections[0].Should().BeOfType<IfMacroSection>().Subject;
         ifSection.Field.Name.ToString().Should().Be("condition");
         ifSection.Blocks.Should().HaveCount(1);
         ifSection.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Hello, World!\r\n");
         block0.ElseSection.Blocks.Should().HaveCount(1);
         block0.ElseSection.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Goodbye, World!\r\n");
     }
     
     [Test]
     public void Parse_WithIfElseOnSameLine_ShouldParseSuccessfully()
     {
         var template = "{{ $if condition }}Hello, World!{{ $else }}Goodbye, World!{{ $end }}";
         var document = Document.Parse(template);
         document.Header.IsEmpty().Should().BeTrue();
         document.Blocks.Should().HaveCount(1);
         var block0 = document.Blocks[0].Should().BeOfType<ConditionalMacro>().Subject;
         block0.IfSections.Should().HaveCount(1);
         block0.ElseSection.Should().NotBeNull();
         var ifSection = block0.IfSections[0].Should().BeOfType<IfMacroSection>().Subject;
         ifSection.Field.Name.ToString().Should().Be("condition");
         ifSection.Blocks.Should().HaveCount(1);
         ifSection.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Hello, World!");
         block0.ElseSection.Blocks.Should().HaveCount(1);
         block0.ElseSection.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Goodbye, World!");
     }
     
     [Test]
     public void Parse_WithIfElseIfAndNoElseOnSeparateLines_ShouldParseSuccessfully()
     {
         var template =
             """
             {{ $if condition1 }}
             Hello, World!
             {{ $elseif condition2 }}
             Goodbye, World!
             {{ $end }}
             """;
         var document = Document.Parse(template);
         document.Header.IsEmpty().Should().BeTrue();
         document.Blocks.Should().HaveCount(1);
         var block0 = document.Blocks[0].Should().BeOfType<ConditionalMacro>().Subject;
         block0.IfSections.Should().HaveCount(2);
         block0.ElseSection.Should().BeNull();
         var ifSection1 = block0.IfSections[0].Should().BeOfType<IfMacroSection>().Subject;
         ifSection1.Field.Name.ToString().Should().Be("condition1");
         ifSection1.Blocks.Should().HaveCount(1);
         ifSection1.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Hello, World!\r\n");
         var ifSection2 = block0.IfSections[1].Should().BeOfType<IfMacroSection>().Subject;
         ifSection2.Field.Name.ToString().Should().Be("condition2");
         ifSection2.Blocks.Should().HaveCount(1);
         ifSection2.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Goodbye, World!\r\n");
     }
     
      [Test]
      public void Parse_WithIfElseIfAndNoElseOnSameLine_ShouldParseSuccessfully()
      {
          var template = "{{ $if condition1 }}Hello, World!{{ $elseif condition2 }}Goodbye, World!{{ $end }}";
          var document = Document.Parse(template);
          document.Header.IsEmpty().Should().BeTrue();
          document.Blocks.Should().HaveCount(1);
          var block0 = document.Blocks[0].Should().BeOfType<ConditionalMacro>().Subject;
          block0.IfSections.Should().HaveCount(2);
          block0.ElseSection.Should().BeNull();
          var ifSection1 = block0.IfSections[0].Should().BeOfType<IfMacroSection>().Subject;
          ifSection1.Field.Name.ToString().Should().Be("condition1");
          ifSection1.Blocks.Should().HaveCount(1);
          ifSection1.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Hello, World!");
          var ifSection2 = block0.IfSections[1].Should().BeOfType<IfMacroSection>().Subject;
          ifSection2.Field.Name.ToString().Should().Be("condition2");
          ifSection2.Blocks.Should().HaveCount(1);
          ifSection2.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Goodbye, World!");
      }
      
      [Test]
      public void Parse_WithIfElseIfAndElseOnSeparateLines_ShouldParseSuccessfully()
      {
          var template =
              """
              {{ $if condition1 }}
              Hello, World!
              {{ $elseif condition2 }}
              Goodbye, World!
              {{ $else }}
              Farewell, World!
              {{ $end }}
              """;
          var document = Document.Parse(template);
          document.Header.IsEmpty().Should().BeTrue();
          document.Blocks.Should().HaveCount(1);
          var block0 = document.Blocks[0].Should().BeOfType<ConditionalMacro>().Subject;
          block0.IfSections.Should().HaveCount(2);
          block0.ElseSection.Should().NotBeNull();
          var ifSection1 = block0.IfSections[0].Should().BeOfType<IfMacroSection>().Subject;
          ifSection1.Field.Name.ToString().Should().Be("condition1");
          ifSection1.Blocks.Should().HaveCount(1);
          ifSection1.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Hello, World!\r\n");
          var ifSection2 = block0.IfSections[1].Should().BeOfType<IfMacroSection>().Subject;
          ifSection2.Field.Name.ToString().Should().Be("condition2");
          ifSection2.Blocks.Should().HaveCount(1);
          ifSection2.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Goodbye, World!\r\n");
          block0.ElseSection.Blocks.Should().HaveCount(1);
          block0.ElseSection.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Farewell, World!\r\n");
      }
      
      [Test]
      public void Parse_WithIfElseIfAndElseOnSameLine_ShouldParseSuccessfully()
      {
          var template = "{{ $if condition1 }}Hello, World!{{ $elseif condition2 }}Goodbye, World!{{ $else }}Farewell, World!{{ $end }}";
          var document = Document.Parse(template);
          document.Header.IsEmpty().Should().BeTrue();
          document.Blocks.Should().HaveCount(1);
          var block0 = document.Blocks[0].Should().BeOfType<ConditionalMacro>().Subject;
          block0.IfSections.Should().HaveCount(2);
          block0.ElseSection.Should().NotBeNull();
          var ifSection1 = block0.IfSections[0].Should().BeOfType<IfMacroSection>().Subject;
          ifSection1.Field.Name.ToString().Should().Be("condition1");
          ifSection1.Blocks.Should().HaveCount(1);
          ifSection1.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Hello, World!");
          var ifSection2 = block0.IfSections[1].Should().BeOfType<IfMacroSection>().Subject;
          ifSection2.Field.Name.ToString().Should().Be("condition2");
          ifSection2.Blocks.Should().HaveCount(1);
          ifSection2.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Goodbye, World!");
          block0.ElseSection.Blocks.Should().HaveCount(1);
          block0.ElseSection.Blocks[0].Should().BeOfType<TextBlock>().Which.Text.ToString().Should().Be("Farewell, World!");
      }
}