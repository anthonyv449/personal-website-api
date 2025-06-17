using System;
using System.Linq;
using System.Reflection;
using personal_website_api;

namespace personal_website_api.Tests;

public class ArticleFunctionsTests
{
    private static string CallExtractValue(string text, string label)
    {
        var method = typeof(ArticleFunctions).GetMethod("ExtractValue", BindingFlags.NonPublic | BindingFlags.Static);
        return (string)method!.Invoke(null, new object?[] { text, label })!;
    }

    private static string CallEstimateReadingTime(string content)
    {
        var method = typeof(ArticleFunctions).GetMethod("EstimateReadingTime", BindingFlags.NonPublic | BindingFlags.Static);
        return (string)method!.Invoke(null, new object?[] { content })!;
    }

    [Fact]
    public void ExtractValue_ReturnsValueAfterLabel()
    {
        const string text = "Summary: Hello\nSEO Tags: tag1";
        var result = CallExtractValue(text, "Summary:");
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ExtractValue_ReturnsEmptyWhenMissing()
    {
        var result = CallExtractValue("No labels here", "Summary:");
        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [InlineData("word", "1 min read")]
    public void EstimateReadingTime_CalculatesMinutes_ForShortText(string content, string expected)
    {
        var result = CallEstimateReadingTime(content);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EstimateReadingTime_CalculatesMinutes_ForLongText()
    {
        var words = string.Join(' ', Enumerable.Repeat("w", 250));
        var result = CallEstimateReadingTime(words);
        Assert.Equal("2 min read", result);
    }
}
