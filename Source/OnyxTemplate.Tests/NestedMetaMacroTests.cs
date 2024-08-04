// OnyxTemplate.Tests
// 
// Copyright 2024 Morten Aune Lyrstad

using FluentAssertions;
using NUnit.Framework;

namespace OnyxTemplate.Tests;

[TestFixture]
public class NestedMetaMacroTests
{
    [Test]
    public void ToString_With3By3Table_ShouldReturnExpectedString()
    {
        var template = new NestedMetaMacroTemplate();
        template.Rows = new[]
        {
            new NestedMetaMacroTemplate.RowsItem { Cells = new[] { "1", "2", "3" } },
            new NestedMetaMacroTemplate.RowsItem { Cells = new[] { "4", "5", "6" } },
            new NestedMetaMacroTemplate.RowsItem { Cells = new[] { "7", "8", "9" } }
        };

        var result = template.ToString();
        result.Should().Be(
            """
            **|1|2|3|**
              |4|5|6|
            --|7|8|9|--

            """
        );
    }
}