// OnyxTemplate.Tests
// 
// Copyright 2024 Morten Aune Lyrstad

using FluentAssertions;
using Mal.OnyxTemplate;
using Mal.OnyxTemplate.DocumentModel;
using NUnit.Framework;

namespace OnyxTemplate.Tests;

[TestFixture]
public class ComplexTest
{
    [Test]
    public void Write_WithNoProperties_ShouldWriteExpectedDocument()
    {
        var template = new ComplexTemplate
        {
            Title = "Document Title",
            Description = "Document Description"
        };
        var result = template.ToString();
        result.Should().Be(
            """
            <html>
                <head>
                    <title>Document Title</title>
                </head>
                <body>
                    <h1>Document Title</h1>
                    <p>Document Description</p>
                </body>
            </html>

            """
        );
    }
    
    public void Write_WithOnlyProperties_ShouldWriteExpectedDocument()
    {
        var template = new ComplexTemplate
        {
            Title = "Document Title",
            Description = "Document Description",
            Properties = new[]
            {
                new ComplexTemplate.PropertyItem { Signature = "Property1", Description = "Property1 Description" },
                new ComplexTemplate.PropertyItem { Signature = "Property2", Description = "Property2 Description" }
            }
        };
        
        var result = template.ToString();
        result.Should().Be(
            """
            <html>
                <head>
                    <title>Document Title</title>
                </head>
                <body>
                    <h1>Document Title</h1>
                    <p>Document Description</p>
                    <h2>Properties</h2>
                    <table>
                        <tr>
                            <td>Property1</td>
                            <td>Property1 Description</td>
                        </tr>
                        <tr>
                            <td>Property2</td>
                            <td>Property2 Description</td>
                        </tr>
                    </table>
                </body>
            </html>

            """
        );
    }
    
    [Test]
    public void Write_WithAllProperties_ShouldWriteExpectedDocument()
    {
        var template = new ComplexTemplate()
        {
            Title = "Document Title",
            Description = "Document Description",
            Fields = new[]
            {
                new ComplexTemplate.FieldItem { Signature = "Field1", Description = "Field1 Description" },
                new ComplexTemplate.FieldItem { Signature = "Field2", Description = "Field2 Description" }
            },
            Events = new[]
            {
                new ComplexTemplate.EventItem { Signature = "Event1", Description = "Event1 Description" },
                new ComplexTemplate.EventItem { Signature = "Event2", Description = "Event2 Description" }
            },
            Properties = new[]
            {
                new ComplexTemplate.PropertyItem { Signature = "Property1", Description = "Property1 Description" },
                new ComplexTemplate.PropertyItem { Signature = "Property2", Description = "Property2 Description" }
            },
            Methods = new[]
            {
                new ComplexTemplate.MethodItem { Signature = "Method1", Description = "Method1 Description" },
                new ComplexTemplate.MethodItem { Signature = "Method2", Description = "Method2 Description" }
            }
        };
        
        var result = template.ToString();
        result.Should().Be(
            """
            <html>
                <head>
                    <title>Document Title</title>
                </head>
                <body>
                    <h1>Document Title</h1>
                    <p>Document Description</p>
                    <h2>Fields</h2>
                    <table>
                        <tr>
                            <td>Field1</td>
                            <td>Field1 Description</td>
                        </tr>
                        <tr>
                            <td>Field2</td>
                            <td>Field2 Description</td>
                        </tr>
                    </table>
                    <h2>Events</h2>
                    <table>
                        <tr>
                            <td>Event1</td>
                            <td>Event1 Description</td>
                        </tr>
                        <tr>
                            <td>Event2</td>
                            <td>Event2 Description</td>
                        </tr>
                    </table>
                    <h2>Properties</h2>
                    <table>
                        <tr>
                            <td>Property1</td>
                            <td>Property1 Description</td>
                        </tr>
                        <tr>
                            <td>Property2</td>
                            <td>Property2 Description</td>
                        </tr>
                    </table>
                    <h2>Methods</h2>
                    <table>
                        <tr>
                            <td>Method1</td>
                            <td>Method1 Description</td>
                        </tr>
                        <tr>
                            <td>Method2</td>
                            <td>Method2 Description</td>
                        </tr>
                    </table>
                </body>
            </html>

            """
        );
    }
}