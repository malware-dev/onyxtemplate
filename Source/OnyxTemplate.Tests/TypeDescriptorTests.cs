// OnyxTemplate.Tests
// 
// Copyright 2024 Morten Aune Lyrstad

using FluentAssertions;
using Mal.OnyxTemplate.DocumentModel;
using NUnit.Framework;

namespace OnyxTemplate.Tests;

[TestFixture]
public class TypeDescriptorTests
{
    [Test]
    public void ToTemplateTypeDescriptor_WithOnlySimpleMacros_GeneratesCorrectTypeDescriptor()
    {
        var template =
            """
            Hello, {{ UserName }}, welcome to the show.
            """;

        var document = Document.Parse(template);
        var typeDescriptor = document.ToTemplateTypeDescriptor();
        typeDescriptor.Should().NotBeNull();
        typeDescriptor.Fields.Length.Should().Be(1);
        typeDescriptor.Fields[0].Name.Should().Be(new Identifier("UserName"));
        typeDescriptor.Fields[0].Type.Should().Be(TemplateFieldType.String);
    }
    
    [Test]
    public void ToTemplateTypeDescriptor_WithConditionalMacros_GeneratesCorrectTypeDescriptor()
    {
        var template =
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
        
        var document = Document.Parse(template);
        var typeDescriptor = document.ToTemplateTypeDescriptor();
        typeDescriptor.Should().NotBeNull();
        typeDescriptor.Fields.Length.Should().Be(3);
        typeDescriptor.Fields[0].Name.Should().Be(new Identifier("IsAdmin"));
        typeDescriptor.Fields[0].Type.Should().Be(TemplateFieldType.Boolean);
        typeDescriptor.Fields[1].Name.Should().Be(new Identifier("UserName"));
        typeDescriptor.Fields[1].Type.Should().Be(TemplateFieldType.String);
        typeDescriptor.Fields[2].Name.Should().Be(new Identifier("WorkTitle"));
        typeDescriptor.Fields[2].Type.Should().Be(TemplateFieldType.String);
    }
    
    [Test]
    public void ToTemplateTypeDescriptor_WithSimpleForEach_GeneratesCorrectTypeDescriptor()
    {
        var template =
            """
            {{ $foreach item in Items }}
                {{ item }} {{ item }}
            {{ $next }}
            """;
        
        var document = Document.Parse(template);
        var typeDescriptor = document.ToTemplateTypeDescriptor();
        typeDescriptor.Should().NotBeNull();
        typeDescriptor.Fields.Length.Should().Be(1);
        typeDescriptor.Fields[0].Name.Should().Be(new Identifier("Items"));
        typeDescriptor.Fields[0].Type.Should().Be(TemplateFieldType.Collection);
        typeDescriptor.Fields[0].ElementType.Should().Be(TemplateFieldType.String);
    }
    
    [Test]
    public void ToTemplateTypeDescriptor_WithComplexForEach_GeneratesCorrectTypeDescriptor()
    {
        var template =
            """
            {{ $foreach item in Items }}
                {{ Name }} {{ Age }}
            {{ $next }}
            """;
        
        var document = Document.Parse(template);
        var typeDescriptor = document.ToTemplateTypeDescriptor();
        typeDescriptor.Should().NotBeNull();
        typeDescriptor.Fields.Length.Should().Be(1);
        typeDescriptor.Fields[0].Name.Should().Be(new Identifier("Items"));
        typeDescriptor.Fields[0].Type.Should().Be(TemplateFieldType.Collection);
        typeDescriptor.Fields[0].ElementType.Should().Be(TemplateFieldType.Complex);
    }
    
    [Test]
    public void ToTemplateTypeDescriptor_WithComplexForEachWithUpReferences_GeneratesCorrectTypeDescriptor()
    {
        var template =
            """
            {{ $foreach product in Products }}
                {{ name }} {{ $if .useDiscount }}{{ discountPrice }}{{ $else }}{{ price }}{{ $end }}
            {{ $next }}
            """;
        
        var document = Document.Parse(template);
        var typeDescriptor = document.ToTemplateTypeDescriptor();
        typeDescriptor.Should().NotBeNull();
        typeDescriptor.Fields.Length.Should().Be(2);
        typeDescriptor.Fields[0].Name.Should().Be(new Identifier("Products"));
        typeDescriptor.Fields[0].Type.Should().Be(TemplateFieldType.Collection);
        typeDescriptor.Fields[0].ElementType.Should().Be(TemplateFieldType.Complex);
        typeDescriptor.Fields[1].Name.Should().Be(new Identifier("UseDiscount"));
        typeDescriptor.Fields[1].Type.Should().Be(TemplateFieldType.Boolean);
        
        var complexType = typeDescriptor.Fields[0].ComplexType;
        complexType.Should().NotBeNull();
        complexType.Name.Should().Be(new Identifier("ProductItem"));
        complexType.Fields.Length.Should().Be(3);
        complexType.Fields[0].Name.Should().Be(new Identifier("DiscountPrice"));
        complexType.Fields[0].Type.Should().Be(TemplateFieldType.String);
        complexType.Fields[1].Name.Should().Be(new Identifier("Name"));
        complexType.Fields[1].Type.Should().Be(TemplateFieldType.String);
        complexType.Fields[2].Name.Should().Be(new Identifier("Price"));
        complexType.Fields[2].Type.Should().Be(TemplateFieldType.String);
    }
}