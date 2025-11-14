namespace tsql_formatter.Tests;

/// <summary>
/// Tests for the TransactSQLFormatterOptions class.
/// </summary>
public class TransactSQLFormatterOptionsTests
{
  [Fact]
  public void Constructor_CreatesOptionsWithDefaultValues()
  {
    // Arrange & Act
    var options = new TransactSQLFormatterOptions();

    // Assert
    Assert.Equal("\t", options.IndentationString);
    Assert.Equal(TransactSQLFormatterOptions.IdentifierStyles.SquareBrackets, options.IdentifierStyle);
    Assert.Equal(TransactSQLFormatterOptions.KeywordCases.Uppercase, options.KeywordCase);
    Assert.Equal(1, options.LinesBetweenStatements);
    Assert.Equal(TransactSQLFormatterOptions.OperatorSpacings.SpaceAround, options.OperatorSpacing);
  }

  [Fact]
  public void IndentationString_CanBeSetToSpaces()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IndentationString = "  "
    };

    // Act & Assert
    Assert.Equal("  ", options.IndentationString);
  }

  [Fact]
  public void IndentationString_CanBeSetToFourSpaces()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IndentationString = "    "
    };

    // Act & Assert
    Assert.Equal("    ", options.IndentationString);
  }

  [Theory]
  [InlineData(TransactSQLFormatterOptions.IdentifierStyles.DoubleQuotes)]
  [InlineData(TransactSQLFormatterOptions.IdentifierStyles.None)]
  [InlineData(TransactSQLFormatterOptions.IdentifierStyles.SquareBrackets)]
  public void IdentifierStyle_CanBeSetToAllValidValues(TransactSQLFormatterOptions.IdentifierStyles style)
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IdentifierStyle = style
    };

    // Act & Assert
    Assert.Equal(style, options.IdentifierStyle);
  }

  [Theory]
  [InlineData(TransactSQLFormatterOptions.KeywordCases.Lowercase)]
  [InlineData(TransactSQLFormatterOptions.KeywordCases.Uppercase)]
  public void KeywordCase_CanBeSetToAllValidValues(TransactSQLFormatterOptions.KeywordCases keywordCase)
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      KeywordCase = keywordCase
    };

    // Act & Assert
    Assert.Equal(keywordCase, options.KeywordCase);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  public void LinesBetweenStatements_CanBeSetToVariousValues(int lines)
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      LinesBetweenStatements = lines
    };

    // Act & Assert
    Assert.Equal(lines, options.LinesBetweenStatements);
  }

  [Theory]
  [InlineData(TransactSQLFormatterOptions.OperatorSpacings.Dense)]
  [InlineData(TransactSQLFormatterOptions.OperatorSpacings.SpaceAround)]
  public void OperatorSpacing_CanBeSetToAllValidValues(TransactSQLFormatterOptions.OperatorSpacings spacing)
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      OperatorSpacing = spacing
    };

    // Act & Assert
    Assert.Equal(spacing, options.OperatorSpacing);
  }
}
