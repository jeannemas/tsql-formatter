# Transact‑SQL Formatter

A no-nonsense Transact-SQL formatter, that just formats code the way you want — fast, predictable, and extensible.

## Features

- Clean, opinionated formatting for `SELECT`/`INSERT`/`DELETE` queries and many expressions.
- Configurable indentation, identifier style, keyword casing, operator spacing and spacing between statements.
- Leverages `Microsoft.SqlServer.Management.SqlParser` for robust parsing.
- Designed to be extended — unimplemented nodes fall back to raw SQL so nothing is lost.

## Quick start

1. Reference the project or library in your app.
2. Configure options and format a script:

```cs
using tsql_formatter;

TransactSQLFormatterOptions options = new TransactSQLFormatterOptions()
{
  IndentationString = "  ",
  IdentifierStyle = TransactSQLFormatterOptions.IdentifierStyles.SquareBrackets,
  KeywordCase = TransactSQLFormatterOptions.KeywordCases.Uppercase,
  LinesBetweenStatements = 1,
  OperatorSpacing = TransactSQLFormatterOptions.OperatorSpacings.SpaceAround,
};

TransactSQLFormatter formatter = new TransactSQLFormatter(options);
string formatted = formatter.FormatTsql("SELECT * FROM dbo.MyTable WHERE Id=1;");

Console.WriteLine(formatted);
```

## Why use this formatter?

- Predictable results — formatting decisions are explicit and configurable.
- Safe — parsing via [Microsoft.SqlServer.Management.SqlParser.SqlCodeDom](https://learn.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.management.sqlparser.sqlcodedom?view=sql-smo-150) avoids brittle regex hacks.
- Lightweight — focused scope avoids heavy opinions and preserves unknown constructs.

## Contributing

Contributions are welcome! Please open issues or pull requests on [GitHub](https://github.com/jeannemas/tsql-formatter).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## TODO

- Implement formatting for missing SQL constructs.
- Add more configuration options for fine-tuning formatting behavior.
- Add support for formatting comments and special SQL syntax.
- Add test coverage with unit tests.
