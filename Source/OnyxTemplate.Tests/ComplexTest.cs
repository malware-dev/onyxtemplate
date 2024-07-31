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
    const string Source =
        """
        {{ $template indented }}
        <html>
            <head>
                <title>{{ Title }}</title>
            </head>
            <body>
                <h1>{{ Title }}</h1>
                <p>{{ Description }}</p>
                {{ $if Fields }}
                <h2>Fields</h2>
                <table>
                {{ $foreach field in Fields }}
                    <tr>
                        <td>{{ Signature }}</td>
                        <td>{{ Description }}</td>
                    </tr>
                {{ $next }}
                </table>
                {{ $end }}
                {{ $if Events }}
                <h2>Events</h2>
                <table>
                {{ $foreach event in Events }}
                    <tr>
                        <td>{{ Signature }}</td>
                        <td>{{ Description }}</td>
                    </tr>
                {{ $next }}
                </table>
                {{ $end }}
                {{ $if Properties }}
                <h2>Properties</h2>
                <table>
                {{ $foreach property in Properties }}
                    <tr>
                        <td>{{ Signature }}</td>
                        <td>{{ Description }}</td>
                    </tr>
                {{ $next }}
                </table>
                {{ $end }}
                {{ $if Methods }}
                <h2>Methods</h2>
                <table>
                {{ $foreach method in Methods }}
                    <tr>
                        <td>{{ Signature }}</td>
                        <td>{{ Description }}</td>
                    </tr>
                {{ $next }}
                </table>
                {{ $end }}
            </body>
        </html>

        """;
    
    [Test]
    public void Write_WithComplexDocument_ShouldWriteDocument()
    {
        var document = Document.Parse(Source);
        var writer = new StringWriter();
        var documentWriter = new DocumentWriter(writer, false);
        documentWriter.Write(document, "OnyxTemplates", new Identifier("MyTemplate"), true);
        var result = writer.ToString();
    }
}