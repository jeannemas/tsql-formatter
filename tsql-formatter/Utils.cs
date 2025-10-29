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
  /// Outputs a debug message to the console if in DEBUG mode.
  /// </summary>
  /// <param name="message">
  /// The debug message.
  /// </param>
  public static void Debug(string message)
  {
#if DEBUG
    Console.WriteLine(message);
#endif
  }
}
