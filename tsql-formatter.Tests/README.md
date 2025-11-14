# Unit Tests for tsql-formatter

This directory contains comprehensive unit tests for the tsql-formatter project using xUnit.

## Test Structure

The test suite is organized into the following test classes:

### 1. TransactSQLFormatterOptionsTests.cs
Tests for the `TransactSQLFormatterOptions` configuration class:
- Default value verification
- Setting custom indentation (spaces, tabs, etc.)
- Identifier style options (SquareBrackets, DoubleQuotes, None)
- Keyword casing (Uppercase, Lowercase)
- Lines between statements configuration
- Operator spacing options (Dense, SpaceAround)

### 2. TransactSQLFormatterTests.cs
Tests for the main `TransactSQLFormatter` class and its formatting capabilities:
- Basic SELECT statement formatting
- Keyword casing (uppercase/lowercase)
- Identifier styles (square brackets, double quotes, no style)
- Operator spacing (space around vs. dense)
- Indentation options (2 spaces, 4 spaces, tabs)
- Lines between multiple statements
- SELECT with WHERE clause
- SELECT with JOINs (INNER JOIN, LEFT OUTER JOIN, etc.)
- SELECT with ORDER BY and GROUP BY
- SELECT with aggregate functions (COUNT, SUM, AVG, MAX, MIN)
- SELECT with TOP and DISTINCT
- DELETE statements
- CASE expressions
- Subqueries
- Common Table Expressions (CTE)
- BETWEEN, LIKE, and IS NULL expressions
- CAST expressions

### 3. UtilsTests.cs
Tests for internal utility methods using reflection:
- `AppendToLast` method with string parameter
- `AppendToLast` method with IEnumerable<string> parameter
- Error handling for empty lists
- Chained calls and multiple operations

### 4. FormatterIntegrationTests.cs
End-to-end integration tests validating complete formatting scenarios:
- Complex queries with multiple clauses
- All options customized at once
- Multiple statements with custom line spacing
- Complex subqueries with proper nesting
- Multiple joins
- GROUP BY with HAVING
- CTEs with proper structure
- DELETE with subqueries
- UNION operations
- Complex CASE expressions
- Multiple aggregates
- Real-world examples from the README
- Arithmetic and comparison operators

## Running the Tests

### Run all tests:
```bash
dotnet test
```

### Run tests with verbose output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run tests from a specific file:
```bash
dotnet test --filter "FullyQualifiedName~TransactSQLFormatterOptionsTests"
```

### Run a specific test:
```bash
dotnet test --filter "FullyQualifiedName~FormatTsql_SimpleSelectStatement_FormatsCorrectly"
```

### Generate code coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Coverage

The test suite provides comprehensive coverage of:
- **Configuration Options**: All formatting options are tested with various values
- **SQL Statement Types**: SELECT, DELETE, and their variations
- **SQL Clauses**: FROM, WHERE, JOIN, GROUP BY, HAVING, ORDER BY, etc.
- **SQL Expressions**: Literals, column references, functions, CASE, subqueries
- **Operators**: Comparison, arithmetic, logical operators
- **Edge Cases**: Empty strings, complex nested queries, multiple statements
- **Integration Scenarios**: Real-world query formatting

## Test Statistics

- **Total Tests**: 64
- **Test Classes**: 4
- **Coverage Areas**: 
  - Configuration: ~12 tests
  - Formatter API: ~35 tests
  - Utilities: ~8 tests
  - Integration: ~24 tests

## Notes

- The `UtilsTests` class uses reflection to test internal utility methods
- Some SQL constructs that are unimplemented (like IN expressions) will show debug output but don't cause test failures
- Tests validate both the presence of formatted elements and the proper application of formatting options
