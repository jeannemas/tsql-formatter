namespace tsql_formatter.Tests;

/// <summary>
/// Tests for the TransactSQLFormatter class.
/// </summary>
public class TransactSQLFormatterTests
{
  [Fact]
  public void FormatTsql_SimpleSelectStatement_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("SELECT", result);
    Assert.Contains("FROM", result);
  }

  [Fact]
  public void FormatTsql_WithUppercaseKeywords_FormatsWithUppercase()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      KeywordCase = TransactSQLFormatterOptions.KeywordCases.Uppercase
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "select * from users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("FROM", result);
    Assert.DoesNotContain("select", result);
    Assert.DoesNotContain("from", result);
  }

  [Fact]
  public void FormatTsql_WithLowercaseKeywords_FormatsWithLowercase()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      KeywordCase = TransactSQLFormatterOptions.KeywordCases.Lowercase
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("select", result);
    Assert.Contains("from", result);
    Assert.DoesNotContain("SELECT", result);
    Assert.DoesNotContain("FROM", result);
  }

  [Fact]
  public void FormatTsql_WithSquareBracketIdentifiers_UsesSquareBrackets()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IdentifierStyle = TransactSQLFormatterOptions.IdentifierStyles.SquareBrackets
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT Id FROM Users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("[Id]", result);
    Assert.Contains("[Users]", result);
  }

  [Fact]
  public void FormatTsql_WithDoubleQuoteIdentifiers_UsesDoubleQuotes()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IdentifierStyle = TransactSQLFormatterOptions.IdentifierStyles.DoubleQuotes
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT Id FROM Users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("\"Id\"", result);
    Assert.Contains("\"Users\"", result);
  }

  [Fact]
  public void FormatTsql_WithNoIdentifierStyle_DoesNotAddQuotes()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IdentifierStyle = TransactSQLFormatterOptions.IdentifierStyles.None
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT Id FROM Users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.DoesNotContain("[Id]", result);
    Assert.DoesNotContain("\"Id\"", result);
    Assert.Contains("Id", result);
  }

  [Fact]
  public void FormatTsql_WithSpaceAroundOperators_AddsSpaces()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      OperatorSpacing = TransactSQLFormatterOptions.OperatorSpacings.SpaceAround
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users WHERE Id=1";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains(" = ", result);
  }

  [Fact]
  public void FormatTsql_WithDenseOperators_NoSpaces()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      OperatorSpacing = TransactSQLFormatterOptions.OperatorSpacings.Dense
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users WHERE Id = 1";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("=", result);
    Assert.DoesNotContain(" = ", result);
  }

  [Fact]
  public void FormatTsql_WithTwoSpaceIndentation_UsesCorrectIndentation()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IndentationString = "  "
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT Id, Name FROM Users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("  ", result); // Should have two-space indentation
  }

  [Fact]
  public void FormatTsql_WithTabIndentation_UsesTabCharacter()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IndentationString = "\t"
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT Id, Name FROM Users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("\t", result); // Should have tab indentation
  }

  [Fact]
  public void FormatTsql_MultipleStatements_AddsLinesBetween()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      LinesBetweenStatements = 2
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users; SELECT * FROM Orders;";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    var lines = result.Split(Environment.NewLine);
    Assert.True(lines.Length > 5); // Should have multiple lines
  }

  [Fact]
  public void FormatTsql_SelectWithWhere_FormatsWithProperStructure()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT Id, Name FROM Users WHERE Active = 1";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("FROM", result);
    Assert.Contains("WHERE", result);
  }

  [Fact]
  public void FormatTsql_SelectWithJoin_FormatsJoinCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT u.Id, o.OrderId FROM Users u INNER JOIN Orders o ON u.Id = o.UserId";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("FROM", result);
    Assert.Contains("INNER JOIN", result);
    Assert.Contains("ON", result);
  }

  [Fact]
  public void FormatTsql_SelectWithOrderBy_FormatsOrderByCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT Id, Name FROM Users ORDER BY Name ASC";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("ORDER BY", result);
    Assert.Contains("ASC", result);
  }

  [Fact]
  public void FormatTsql_SelectWithGroupBy_FormatsGroupByCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT Country, COUNT(*) FROM Users GROUP BY Country";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("GROUP BY", result);
    Assert.Contains("COUNT", result);
  }

  [Fact]
  public void FormatTsql_DeleteStatement_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "DELETE FROM Users WHERE Id = 1";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("DELETE", result);
    Assert.Contains("Users", result);
    Assert.Contains("WHERE", result);
  }

  [Fact]
  public void FormatTsql_SelectWithCaseExpression_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT CASE WHEN Age >= 18 THEN 'Adult' ELSE 'Minor' END FROM Users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("CASE", result);
    Assert.Contains("WHEN", result);
    Assert.Contains("THEN", result);
    Assert.Contains("ELSE", result);
    Assert.Contains("END", result);
  }

  [Fact]
  public void FormatTsql_SelectWithSubquery_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users WHERE Id IN (SELECT UserId FROM Orders)";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("WHERE", result);
    Assert.Contains("IN", result);
  }

  [Fact]
  public void FormatTsql_SelectWithAggregate_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT MAX(Salary), MIN(Salary), AVG(Salary) FROM Employees";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("MAX", result);
    Assert.Contains("MIN", result);
    Assert.Contains("AVG", result);
  }

  [Fact]
  public void FormatTsql_EmptyString_ReturnsEmptyOrMinimal()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.NotNull(result);
  }

  [Fact]
  public void FormatTsql_SelectWithTop_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT TOP 10 * FROM Users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("TOP", result);
  }

  [Fact]
  public void FormatTsql_SelectDistinct_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT DISTINCT Country FROM Users";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("DISTINCT", result);
  }

  [Fact]
  public void FormatTsql_WithCommonTableExpression_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "WITH UserCTE AS (SELECT * FROM Users) SELECT * FROM UserCTE";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("WITH", result);
    Assert.Contains("AS", result);
    Assert.Contains("SELECT", result);
  }

  [Fact]
  public void FormatTsql_SelectWithBetween_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users WHERE Age BETWEEN 18 AND 65";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("BETWEEN", result);
    Assert.Contains("AND", result);
  }

  [Fact]
  public void FormatTsql_SelectWithLike_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users WHERE Name LIKE 'John%'";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("LIKE", result);
  }

  [Fact]
  public void FormatTsql_SelectWithIsNull_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users WHERE Email IS NULL";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("IS NULL", result);
  }

  [Fact]
  public void FormatTsql_SelectWithCast_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT CAST(Price AS DECIMAL(10,2)) FROM Products";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("CAST", result);
    Assert.Contains("AS", result);
  }
}
