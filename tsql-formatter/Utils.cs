using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using System.Text.Json;

namespace tsql_formatter;

internal class Utils
{
  /// <summary>
  /// Appends the given string to the last string in the list.
  /// </summary>
  /// <param name="list">
  /// The list to append to.
  /// </param>
  /// <param name="str">
  /// The string to append.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// Thrown if the list is empty.
  /// </exception>
  public static void AppendToLast(List<string> list, string str)
  {
    if (list.Count == 0)
    {
      throw new InvalidOperationException("Cannot append to last string of an empty list.");
    }

    int lastIndex = list.Count - 1;
    string lastStr = list[lastIndex];

    lastStr += str;

    list[lastIndex] = lastStr;
  }

  /// <summary>
  /// Appends the first of the string to the last string in the list, then appends the next strings as new entries.
  /// </summary>
  /// <param name="list">
  /// The list to append to.
  /// </param>
  /// <param name="strings">
  /// The strings to append.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// Thrown if the list is empty.
  /// </exception>
  public static void AppendToLast(List<string> list, IEnumerable<string> strings)
  {
    if (list.Count == 0)
    {
      throw new InvalidOperationException("Cannot append to last string of an empty list.");
    }

    if (!strings.Any())
    {
      return;
    }

    int lastIndex = list.Count - 1;
    string lastStr = list[lastIndex];

    lastStr += strings.First();

    list[lastIndex] = lastStr;

    list.AddRange(strings.Skip(1));
  }

  /// <summary>
  /// Outputs a debug message to the console if in DEBUG mode.
  /// </summary>
  /// <param name="message">
  /// The debug message.
  /// </param>
  /// <param name="args">
  /// The debug message arguments.
  /// </param>
  public static void Debug(string message, params object?[]? args)
  {
#if DEBUG
    Console.WriteLine(message, args);
#endif
  }
}

public static class SqlCodeObjectExtension
{
  private static readonly JsonSerializerOptions jsonSerializerOptions = new()
  {
    WriteIndented = true,
  };

  /// <summary>
  /// Serializes the given SqlCodeObject into a JSON string.
  /// </summary>
  /// <param name="sqlCodeObject">
  /// The SqlCodeObject to serialize.
  /// </param>
  /// <returns>
  /// The JSON string representation of the SqlCodeObject.
  /// </returns>
  public static string Json(this SqlCodeObject sqlCodeObject)
  {
    IDictionary<string, object?> dictionary = sqlCodeObject
      .GetType()
      .GetProperties()
      .Where(property =>
        (
          property.PropertyType.IsSubclassOf(typeof(SqlCodeObject)) // Include SqlCodeObject properties
          && property.Name != nameof(SqlCodeObject.Statement) // Omit redundant property
        )
        || property.PropertyType.IsEnum // Include enum properties
        || property.Name == nameof(SqlCodeObject.Sql) // Include Sql property
      )
      .ToDictionary(
        property => property.Name,
        property =>
          property.GetValue(sqlCodeObject) switch
          {
            SqlCodeObject codeObject => JsonSerializer.Deserialize<object>(codeObject.Json()),
            Enum enumValue => enumValue.ToString(),
            string s => s,
            null => null,
            _ => null,
          }
      );

    return JsonSerializer.Serialize(dictionary, jsonSerializerOptions);
  }
}
