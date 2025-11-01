using Microsoft.SqlServer.Management.SqlParser.Parser;

namespace tsql_formatter;

/// <summary>
/// Formats T-SQL scripts according to specified options.
/// </summary>
/// <param name="options">
/// The formatting options.
/// </param>
public class TransactSQLFormatter(TransactSQLFormatterOptions options)
{
  /// <summary>
  /// The internal formatter instance.
  /// </summary>
  private readonly Formatter Formatter = new(options);

  /// <summary>
  /// Formats the provided T-SQL script according to the configured options.
  /// </summary>
  /// <param name="tsqlScript">
  /// The T-SQL script to format.
  /// </param>
  /// <returns>
  /// The formatted T-SQL script.
  /// </returns>
  public string FormatTsql(string tsqlScript)
  {
    ParseResult parseResult = Parser.Parse(tsqlScript);

    return Formatter.ParseResult(parseResult);
  }
}
