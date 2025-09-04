namespace backend.Tests;

public class BasicTest
{
    [Xunit.Fact]
    public void SimpleAddition_ShouldWork()
    {
        // Arrange
        var a = 2;
        var b = 3;
        
        // Act
        var result = a + b;
        
        // Assert
        Xunit.Assert.Equal(5, result);
    }
    
    [Xunit.Theory]
    [Xunit.InlineData(1, 2, 3)]
    [Xunit.InlineData(0, 0, 0)]
    [Xunit.InlineData(-1, 1, 0)]
    public void Addition_WithDifferentValues_ShouldWork(int a, int b, int expected)
    {
        // Act
        var result = a + b;
        
        // Assert
        Xunit.Assert.Equal(expected, result);
    }
}
