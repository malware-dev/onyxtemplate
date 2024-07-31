// OnyxTemplate.Tests
// 
// Copyright 2024 Morten Aune Lyrstad

using FluentAssertions;
using NUnit.Framework;

namespace OnyxTemplate.Tests;

[TestFixture]
public class GeneratorTests
{
    [Test]
    public void ToString_WithIndentationTemplate_GeneratesAsExpected()
    {
          var template = new T001
          {
              Content = "Welcome\r\nBienvenue\r\nVelkommen\r\nAloha"
          };
          var result = template.ToString();
          result.Should().Be(
              """
              Items: Welcome
                     Bienvenue
                     Velkommen
                     Aloha

              """);
    }
    
    [Test]
    public void ToString_WithInlineForEachAndInlineCondition_GeneratesAsExpected()
    {
          var template = new T002
          {
              Animals = new List<string> { "Dog", "Cat", "Bird", "Fish" }
          };
          var result = template.ToString();
          result.Should().Be("Animals: Dog, Cat, Bird, Fish\r\n");
    }
}