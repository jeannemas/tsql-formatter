using System.Reflection;

namespace tsql_formatter.Tests;

/// <summary>
/// Tests for the Utils class using reflection since it's internal.
/// </summary>
public class UtilsTests
{
  private readonly Type _utilsType;

  public UtilsTests()
  {
    // Get the internal Utils type through reflection
    var assembly = typeof(TransactSQLFormatter).Assembly;
    _utilsType = assembly.GetType("tsql_formatter.Utils")!;
    Assert.NotNull(_utilsType);
  }

  [Fact]
  public void AppendToLast_WithNonEmptyList_AppendsToLastElement()
  {
    // Arrange
    var list = new List<string> { "Hello" };
    var method = _utilsType.GetMethod("AppendToLast", 
      BindingFlags.Public | BindingFlags.Static,
      null,
      new[] { typeof(List<string>), typeof(string) },
      null);

    // Act
    method!.Invoke(null, new object[] { list, " World" });

    // Assert
    Assert.Single(list);
    Assert.Equal("Hello World", list[0]);
  }

  [Fact]
  public void AppendToLast_WithEmptyList_ThrowsInvalidOperationException()
  {
    // Arrange
    var list = new List<string>();
    var method = _utilsType.GetMethod("AppendToLast",
      BindingFlags.Public | BindingFlags.Static,
      null,
      new[] { typeof(List<string>), typeof(string) },
      null);

    // Act & Assert
    var exception = Assert.Throws<TargetInvocationException>(() =>
      method!.Invoke(null, new object[] { list, "Test" })
    );
    Assert.IsType<InvalidOperationException>(exception.InnerException);
  }

  [Fact]
  public void AppendToLast_WithMultipleStrings_AppendsFirstToLastAndAddsRest()
  {
    // Arrange
    var list = new List<string> { "Start" };
    var stringsToAppend = new List<string> { " Middle", "End" };
    var method = _utilsType.GetMethod("AppendToLast",
      BindingFlags.Public | BindingFlags.Static,
      null,
      new[] { typeof(List<string>), typeof(IEnumerable<string>) },
      null);

    // Act
    method!.Invoke(null, new object[] { list, stringsToAppend });

    // Assert
    Assert.Equal(2, list.Count);
    Assert.Equal("Start Middle", list[0]);
    Assert.Equal("End", list[1]);
  }

  [Fact]
  public void AppendToLast_WithEmptyEnumerable_DoesNotModifyList()
  {
    // Arrange
    var list = new List<string> { "Test" };
    var stringsToAppend = new List<string>();
    var method = _utilsType.GetMethod("AppendToLast",
      BindingFlags.Public | BindingFlags.Static,
      null,
      new[] { typeof(List<string>), typeof(IEnumerable<string>) },
      null);

    // Act
    method!.Invoke(null, new object[] { list, stringsToAppend });

    // Assert
    Assert.Single(list);
    Assert.Equal("Test", list[0]);
  }

  [Fact]
  public void AppendToLast_WithMultipleElements_OnlyModifiesLast()
  {
    // Arrange
    var list = new List<string> { "First", "Second", "Third" };
    var method = _utilsType.GetMethod("AppendToLast",
      BindingFlags.Public | BindingFlags.Static,
      null,
      new[] { typeof(List<string>), typeof(string) },
      null);

    // Act
    method!.Invoke(null, new object[] { list, " Modified" });

    // Assert
    Assert.Equal(3, list.Count);
    Assert.Equal("First", list[0]);
    Assert.Equal("Second", list[1]);
    Assert.Equal("Third Modified", list[2]);
  }

  [Fact]
  public void AppendToLast_WithEmptyString_AddsEmptyString()
  {
    // Arrange
    var list = new List<string> { "Test" };
    var method = _utilsType.GetMethod("AppendToLast",
      BindingFlags.Public | BindingFlags.Static,
      null,
      new[] { typeof(List<string>), typeof(string) },
      null);

    // Act
    method!.Invoke(null, new object[] { list, "" });

    // Assert
    Assert.Single(list);
    Assert.Equal("Test", list[0]);
  }

  [Fact]
  public void AppendToLast_ChainedCalls_WorksCorrectly()
  {
    // Arrange
    var list = new List<string> { "A" };
    var method = _utilsType.GetMethod("AppendToLast",
      BindingFlags.Public | BindingFlags.Static,
      null,
      new[] { typeof(List<string>), typeof(string) },
      null);

    // Act
    method!.Invoke(null, new object[] { list, "B" });
    method!.Invoke(null, new object[] { list, "C" });
    method!.Invoke(null, new object[] { list, "D" });

    // Assert
    Assert.Single(list);
    Assert.Equal("ABCD", list[0]);
  }
}
