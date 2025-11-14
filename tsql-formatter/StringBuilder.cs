namespace tsql_formatter;

public class StringBuilder(IEnumerable<string>? initialLines = null)
{
  /// <summary>
  /// The lines in the string builder.
  /// </summary>
  private readonly List<string> lines = [.. initialLines ?? [string.Empty]];

  /// <summary>
  /// Indicates whether the string builder contains multiple lines.
  /// </summary>
  public bool IsMultiLine => lines.Count > 1;

  /// <summary>
  /// Gets the lines in the string builder.
  /// </summary>
  public IEnumerable<string> Lines => lines;

  /// <summary>
  /// Adds a new empty line.
  /// </summary>
  /// <returns>
  /// The string builder.
  /// </returns>
  public StringBuilder AddNewLine()
  {
    return AddNewLine(string.Empty);
  }

  /// <summary>
  /// Adds a new line with the given string.
  /// </summary>
  /// <param name="str">
  /// The string to add.
  /// </param>
  /// <returns>
  /// The string builder.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown if the provided string is null.
  /// </exception>
  public StringBuilder AddNewLine(string str)
  {
    ArgumentNullException.ThrowIfNull(str);
    lines.Add(str);

    return this;
  }

  /// <summary>
  /// Adds a new line with the given StringBuilder.
  /// </summary>
  /// <param name="stringBuilder">
  /// The string builder to add.
  /// </param>
  /// <returns>
  /// The string builder.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown if the provided StringBuilder is multi-line.
  /// </exception>
  public StringBuilder AddNewLine(StringBuilder stringBuilder)
  {
    if (stringBuilder.IsMultiLine)
    {
      throw new InvalidOperationException("Cannot add multi-line StringBuilder as a single line.");
    }

    return AddNewLine(stringBuilder.ToString());
  }

  /// <summary>
  /// Adds new lines with the given strings.
  /// </summary>
  /// <param name="strs">
  /// The strings to add.
  /// </param>
  /// <returns>
  /// The string builder.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown if any of the provided strings is null.
  /// </exception>
  public StringBuilder AddNewLines(IEnumerable<string> strs)
  {
    foreach (string str in strs)
    {
      ArgumentNullException.ThrowIfNull(str);
      lines.Add(str);
    }

    return this;
  }

  /// <summary>
  /// Appends the given string to the last line.
  /// </summary>
  /// <param name="str">
  /// The string to append.
  /// </param>
  /// <returns>
  /// The string builder.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown if the string builder is empty.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown if the provided string is null.
  /// </exception>
  public StringBuilder AppendToLastLine(string str)
  {
    if (lines.Count == 0)
    {
      throw new InvalidOperationException("Cannot append to last line of an empty list.");
    }

    ArgumentNullException.ThrowIfNull(str);

    int lastIndex = lines.Count - 1;

    lines[lastIndex] = $"{lines[lastIndex]}{str}";

    return this;
  }

  /// <summary>
  /// Appends the given StringBuilder to the last line. If the given StringBuilder is multi-line,
  /// the first line is appended to the last line of this StringBuilder, and the remaining lines
  /// are added as new lines.
  /// </summary>
  /// <param name="stringBuilder">
  /// The string builder to append.
  /// </param>
  /// <returns>
  /// The string builder.
  /// </returns>
  public StringBuilder AppendToLastLine(StringBuilder stringBuilder)
  {
    if (lines.Count == 0)
    {
      throw new InvalidOperationException("Cannot append to last line of an empty list.");
    }

    if (stringBuilder.IsMultiLine)
    {
      int lastIndex = lines.Count - 1;

      lines[lastIndex] = $"{lines[lastIndex]}{stringBuilder.lines.First()}";

      lines.AddRange(stringBuilder.lines.Skip(1));

      return this;
    }

    return AppendToLastLine(stringBuilder.ToString());
  }

  /// <summary>
  /// Returns the string representation of the string builder.
  /// </summary>
  /// <returns>
  /// The string representation of the string builder.
  /// </returns>
  public override string ToString()
  {
    return string.Join(Environment.NewLine, lines);
  }
}
