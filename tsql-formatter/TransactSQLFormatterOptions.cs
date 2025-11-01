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
  public KeywordCases KeywordCase = KeywordCases.Uppercase;

  /// <summary>
  /// Gets or sets the number of lines between SQL statements.
  /// 
  /// Default is 1.
  /// </summary>
  public int LinesBetweenStatements = 1;

  /// <summary>
  /// Gets or sets the spacing style around operators.
  /// 
  /// Default is space around.
  /// </summary>
  public OperatorSpacings OperatorSpacing = OperatorSpacings.SpaceAround;

  public enum IdentifierStyles
  {
    /// <summary>
    /// Uses double quotes for SQL identifiers.
    /// </summary>
    DoubleQuotes,

    /// <summary>
    /// No special styling for SQL identifiers.
    /// </summary>
    None,

    /// <summary>
    /// Uses square brackets for SQL identifiers.
    /// </summary>
    SquareBrackets,
  }

  public enum KeywordCases
  {
    /// <summary>
    /// Uses lowercase for SQL keywords.
    /// </summary>
    Lowercase,

    /// <summary>
    /// Uses uppercase for SQL keywords.
    /// </summary>
    Uppercase,
  }

  public enum OperatorSpacings
  {
    /// <summary>
    /// No spaces around operators.
    /// </summary>
    Dense,

    /// <summary>
    /// Spaces around operators.
    /// </summary>
    SpaceAround,
  }
}
