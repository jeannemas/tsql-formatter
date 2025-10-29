namespace tsql_formatter;

public class TransactSQLFormatterOptions
{
  /// <summary>
  /// Gets or sets the string used for indentation.
  /// 
  /// Default is a tab character.
  /// </summary>
  public string IndentationString = "\t";

  /// <summary>
  /// Gets or sets the style for SQL identifiers.
  /// 
  /// Default is square brackets.
  /// </summary>
  public IdentifierStyles IdentifierStyle = IdentifierStyles.SquareBrackets;

  /// <summary>
  /// Gets or sets the casing for SQL keywords.
  /// 
  /// Default is uppercase.
  /// </summary>
  public KeywordCasing KeywordCase = KeywordCasing.Uppercase;

  public enum IdentifierStyles
  {
    DoubleQuotes,
    None,
    SquareBrackets,
  }

  public enum KeywordCasing
  {
    Lowercase,
    Uppercase,
  }
}
