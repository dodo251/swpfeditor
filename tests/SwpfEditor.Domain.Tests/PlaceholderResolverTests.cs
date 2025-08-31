using Xunit;
using SwpfEditor.Domain.Services;

namespace SwpfEditor.Domain.Tests;

public class PlaceholderResolverTests
{
    [Fact]
    public void ResolvePlaceholder_ShouldRespectPriorityOrder()
    {
        // Arrange
        var resolver = new PlaceholderResolver();
        var placeholderName = "TEST_VALUE";
        
        // Set values at different priority levels (lower priority first)
        resolver.SetEnvironmentVariable(placeholderName, "env_value");
        resolver.SetTestConfigurationValue(placeholderName, "config_value");
        resolver.SetConstant(placeholderName, "constant_value");
        resolver.SetVariable(placeholderName, "variable_value");
        resolver.SetInput(placeholderName, "input_value");
        resolver.SetExtractedValue(placeholderName, "extracted_value");
        resolver.SetRuntimeLocal(placeholderName, "runtime_value");

        // Act
        var result = resolver.ResolvePlaceholder(placeholderName);

        // Assert - should return highest priority value (runtime)
        Assert.Equal("runtime_value", result);
    }

    [Fact]
    public void ResolvePlaceholder_ShouldFallbackThroughPriorityLevels()
    {
        // Arrange
        var resolver = new PlaceholderResolver();
        var placeholderName = "TEST_VALUE";
        
        // Set only lower priority values
        resolver.SetEnvironmentVariable(placeholderName, "env_value");
        resolver.SetTestConfigurationValue(placeholderName, "config_value");
        resolver.SetConstant(placeholderName, "constant_value");

        // Act
        var result = resolver.ResolvePlaceholder(placeholderName);

        // Assert - should return highest available priority (constant)
        Assert.Equal("constant_value", result);
    }

    [Fact]
    public void ResolvePlaceholder_ShouldReturnNullForUnknownPlaceholder()
    {
        // Arrange
        var resolver = new PlaceholderResolver();

        // Act
        var result = resolver.ResolvePlaceholder("UNKNOWN_PLACEHOLDER");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ResolveText_ShouldReplaceMultiplePlaceholders()
    {
        // Arrange
        var resolver = new PlaceholderResolver();
        resolver.SetConstant("NAME", "John");
        resolver.SetConstant("AGE", "30");
        
        var text = "Hello ${NAME}, you are ${AGE} years old!";

        // Act
        var result = resolver.ResolveText(text);

        // Assert
        Assert.Equal("Hello John, you are 30 years old!", result);
    }

    [Fact]
    public void GetUnresolvedPlaceholders_ShouldReturnMissingPlaceholders()
    {
        // Arrange
        var resolver = new PlaceholderResolver();
        resolver.SetConstant("KNOWN", "value");
        
        var text = "Known: ${KNOWN}, Unknown: ${UNKNOWN1} and ${UNKNOWN2}";

        // Act
        var unresolved = resolver.GetUnresolvedPlaceholders(text);

        // Assert
        Assert.Equal(2, unresolved.Count);
        Assert.Contains("UNKNOWN1", unresolved);
        Assert.Contains("UNKNOWN2", unresolved);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("No placeholders here", 0)]
    [InlineData("${SINGLE}", 1)]
    [InlineData("${FIRST} and ${SECOND}", 2)]
    [InlineData("${DUPLICATE} and ${DUPLICATE}", 1)] // Should deduplicate
    public void GetUnresolvedPlaceholders_ShouldHandleVariousCases(string text, int expectedCount)
    {
        // Arrange
        var resolver = new PlaceholderResolver();

        // Act
        var unresolved = resolver.GetUnresolvedPlaceholders(text);

        // Assert
        Assert.Equal(expectedCount, unresolved.Count);
    }
}