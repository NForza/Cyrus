using DemoApp.Contracts;
using FluentAssertions;

namespace DemoApp.WebApi.Tests;

public class ContractTests
{
    [Fact]
    public void Name_That_Has_No_Invalid_Characters_Should_Be_Valid()
    {
        // Arrange
        var name = new Name("John Doe");
        // Act 
        var isValid = name.IsValid();
        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Name_That_Has_Invalid_Characters_Should_Not_Be_Valid()
    {
        // Arrange
        var name = new Name("X9");
        // Act 
        var isValid = name.IsValid();
        // Assert
        isValid.Should().BeFalse();
    }
}
