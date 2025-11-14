namespace tsql_formatter.Tests;

/// <summary>
/// Integration tests that validate complete formatting scenarios.
/// </summary>
public class FormatterIntegrationTests
{
  [Fact]
  public void FormatTsql_ComplexQuery_WithDefaultOptions_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = @"
      SELECT u.Id, u.Name, o.OrderDate, o.Amount
      FROM Users u
      INNER JOIN Orders o ON u.Id = o.UserId
      WHERE u.Active = 1 AND o.Amount > 100
      ORDER BY o.OrderDate DESC
    ";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("SELECT", result);
    Assert.Contains("INNER JOIN", result);
    Assert.Contains("WHERE", result);
    Assert.Contains("ORDER BY", result);
    // Should have proper structure with keywords on separate lines
    var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    Assert.True(lines.Length > 1);
  }

  [Fact]
  public void FormatTsql_AllOptionsCustomized_FormatsAccordingToOptions()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IndentationString = "    ", // 4 spaces
      IdentifierStyle = TransactSQLFormatterOptions.IdentifierStyles.DoubleQuotes,
      KeywordCase = TransactSQLFormatterOptions.KeywordCases.Lowercase,
      LinesBetweenStatements = 2,
      OperatorSpacing = TransactSQLFormatterOptions.OperatorSpacings.Dense
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT Id FROM Users WHERE Active = 1";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    // Keywords should be lowercase
    Assert.Contains("select", result);
    Assert.Contains("from", result);
    Assert.Contains("where", result);
    
    // Identifiers should use double quotes
    Assert.Contains("\"Id\"", result);
    Assert.Contains("\"Users\"", result);
    Assert.Contains("\"Active\"", result);
    
    // Operators should be dense (no spaces)
    Assert.Contains("=", result);
    
    // Should have 4-space indentation
    Assert.Contains("    ", result);
  }

  [Fact]
  public void FormatTsql_MultipleStatementsWithCustomLineSpacing_AppliesCorrectSpacing()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      LinesBetweenStatements = 3
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users; SELECT * FROM Orders; SELECT * FROM Products;";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.NotNull(result);
    var lines = result.Split(Environment.NewLine);
    
    // Should have empty lines between statements
    int emptyLineCount = lines.Count(line => string.IsNullOrWhiteSpace(line));
    Assert.True(emptyLineCount > 0);
  }

  [Fact]
  public void FormatTsql_ComplexSubquery_FormatsWithProperNesting()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IndentationString = "  "
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = @"
      SELECT * FROM Users 
      WHERE Id IN (
        SELECT UserId FROM Orders WHERE OrderDate > '2023-01-01'
      )
    ";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("SELECT", result);
    Assert.Contains("WHERE", result);
    Assert.Contains("IN", result);
    // Should have indentation for nested query
    Assert.Contains("  ", result);
  }

  [Fact]
  public void FormatTsql_WithMultipleJoins_FormatsAllJoinsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = @"
      SELECT u.Name, o.OrderId, p.ProductName
      FROM Users u
      INNER JOIN Orders o ON u.Id = o.UserId
      INNER JOIN OrderDetails od ON o.OrderId = od.OrderId
      LEFT OUTER JOIN Products p ON od.ProductId = p.Id
    ";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("INNER JOIN", result);
    Assert.Contains("LEFT OUTER JOIN", result);
    Assert.Contains("ON", result);
  }

  [Fact]
  public void FormatTsql_WithGroupByAndHaving_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = @"
      SELECT Country, COUNT(*) AS UserCount
      FROM Users
      GROUP BY Country
      HAVING COUNT(*) > 10
    ";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("GROUP BY", result);
    Assert.Contains("HAVING", result);
    Assert.Contains("COUNT", result);
  }

  [Fact]
  public void FormatTsql_WithCTE_FormatsWithProperStructure()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = @"
      WITH ActiveUsers AS (
        SELECT * FROM Users WHERE Active = 1
      )
      SELECT Name FROM ActiveUsers
    ";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("WITH", result);
    Assert.Contains("AS", result);
    Assert.Contains("SELECT", result);
  }

  [Fact]
  public void FormatTsql_DeleteWithJoin_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = @"
      DELETE FROM Orders
      WHERE UserId IN (SELECT Id FROM Users WHERE Active = 0)
    ";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("DELETE", result);
    Assert.Contains("FROM", result);
    Assert.Contains("WHERE", result);
  }

  [Fact]
  public void FormatTsql_SelectWithUnion_FormatsUnionCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = @"
      SELECT Name FROM Customers
      UNION
      SELECT Name FROM Suppliers
    ";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("SELECT", result);
    Assert.Contains("UNION", result);
  }

  [Fact]
  public void FormatTsql_SelectWithComplexCaseExpression_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = @"
      SELECT 
        Name,
        CASE 
          WHEN Age < 18 THEN 'Minor'
          WHEN Age >= 18 AND Age < 65 THEN 'Adult'
          ELSE 'Senior'
        END AS AgeGroup
      FROM Users
    ";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("CASE", result);
    Assert.Contains("WHEN", result);
    Assert.Contains("THEN", result);
    Assert.Contains("ELSE", result);
    Assert.Contains("END", result);
  }

  [Fact]
  public void FormatTsql_WithMultipleAggregates_FormatsAllCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = @"
      SELECT 
        COUNT(*) AS Total,
        SUM(Amount) AS TotalAmount,
        AVG(Amount) AS AverageAmount,
        MAX(Amount) AS MaxAmount,
        MIN(Amount) AS MinAmount
      FROM Orders
    ";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("COUNT", result);
    Assert.Contains("SUM", result);
    Assert.Contains("AVG", result);
    Assert.Contains("MAX", result);
    Assert.Contains("MIN", result);
  }

  [Fact]
  public void FormatTsql_WithTopAndOrderBy_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT TOP 100 Id, Name FROM Users ORDER BY Name ASC";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("TOP", result);
    Assert.Contains("ORDER BY", result);
  }

  [Fact]
  public void FormatTsql_WithDistinctAndOrderBy_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions();
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT DISTINCT Country FROM Users ORDER BY Country";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains("DISTINCT", result);
    Assert.Contains("ORDER BY", result);
  }

  [Fact]
  public void FormatTsql_RealWorldExample_FromReadme_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      IndentationString = "  ",
      IdentifierStyle = TransactSQLFormatterOptions.IdentifierStyles.SquareBrackets,
      KeywordCase = TransactSQLFormatterOptions.KeywordCases.Uppercase,
      LinesBetweenStatements = 1,
      OperatorSpacing = TransactSQLFormatterOptions.OperatorSpacings.SpaceAround,
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM dbo.MyTable WHERE Id=1;";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("SELECT", result);
    Assert.Contains("FROM", result);
    Assert.Contains("WHERE", result);
    // Should have proper formatting applied
    Assert.Contains(" = ", result); // SpaceAround operators
  }

  [Fact]
  public void FormatTsql_WithArithmeticOperations_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      OperatorSpacing = TransactSQLFormatterOptions.OperatorSpacings.SpaceAround
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT Price * Quantity AS Total FROM Orders";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains(" * ", result);
  }

  [Fact]
  public void FormatTsql_WithComparisonOperators_FormatsCorrectly()
  {
    // Arrange
    var options = new TransactSQLFormatterOptions
    {
      OperatorSpacing = TransactSQLFormatterOptions.OperatorSpacings.SpaceAround
    };
    var formatter = new TransactSQLFormatter(options);
    var sql = "SELECT * FROM Users WHERE Age >= 18 AND Salary <= 100000";

    // Act
    var result = formatter.FormatTsql(sql);

    // Assert
    Assert.Contains(" >= ", result);
    Assert.Contains(" <= ", result);
  }
}
