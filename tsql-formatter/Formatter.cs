using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace tsql_formatter;

/// <summary>
/// The base class for formatters.
/// </summary>
/// <param name="options">
/// The formatting options.
/// </param>
internal class Formatter(TransactSQLFormatterOptions options)
{
  /// <summary>
  /// The formatting options.
  /// </summary>
  private readonly TransactSQLFormatterOptions Options = options;

  /// <summary>
  /// Formats a SQL batch.
  /// </summary>
  /// <param name="batch">
  /// The SQL batch to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted batch to.
  /// </param>
  public void Batch(SqlBatch batch, ref List<string> lines)
  {
    foreach (SqlStatement statement in batch.Statements)
    {
      Statement(statement, ref lines);
    }
  }

  /// <summary>
  /// Formats a binary boolean expression.
  /// </summary>
  /// <param name="binaryBooleanExpression">
  /// The binary boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted binary boolean expression to.
  /// </param>
  public void BinaryBooleanExpression(SqlBinaryBooleanExpression binaryBooleanExpression, ref List<string> lines)
  {
    List<string> leftExpressionLines = [];

    BooleanExpression(binaryBooleanExpression.Left, ref leftExpressionLines);

    if (leftExpressionLines.Count > 1)
    {
      lines.Add("(");
      lines.AddRange(leftExpressionLines.Select(line => $"{Indent()}{line}"));
      lines.Add(")");
    }
    else
    {
      lines.Add(leftExpressionLines.First());
    }

    lines.Add($"{BooleanOperatorType(binaryBooleanExpression.Operator)} ");

    List<string> rightExpressionLines = [];

    BooleanExpression(binaryBooleanExpression.Right, ref rightExpressionLines);

    if (rightExpressionLines.Count > 1)
    {
      Utils.AppendToLast(lines, "(");
      lines.AddRange(rightExpressionLines.Select(line => $"{Indent()}{line}"));
      lines.Add(")");
    }
    else
    {
      Utils.AppendToLast(lines, rightExpressionLines.First());
    }
  }

  /// <summary>
  /// Formats a boolean expression.
  /// </summary>
  /// <param name="booleanExpression">
  /// The boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted boolean expression to.
  /// </param>
  public void BooleanExpression(SqlBooleanExpression booleanExpression, ref List<string> lines)
  {
    switch (booleanExpression)
    {
      case SqlBinaryBooleanExpression binaryBooleanExpression:
        {
          BinaryBooleanExpression(binaryBooleanExpression, ref lines);

          break;
        }

      case SqlComparisonBooleanExpression comparisonBooleanExpression:
        {
          ComparisonBooleanExpression(comparisonBooleanExpression, ref lines);

          break;
        }

      default:
        {
          Utils.Debug($"Unrecognized boolean expression type: {booleanExpression.GetType().FullName}");
          lines.Add(booleanExpression.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a boolean operator type.
  /// </summary>
  /// <param name="booleanOperatorType">
  /// The boolean operator type to format.
  /// </param>
  /// <returns>
  /// The formatted boolean operator type.
  /// </returns>
  public string BooleanOperatorType(SqlBooleanOperatorType booleanOperatorType)
  {
    return booleanOperatorType switch
    {
      SqlBooleanOperatorType.And => Keyword(Keywords.AND),
      SqlBooleanOperatorType.Or => Keyword(Keywords.OR),
      _ => string.Empty,
    };
  }

  /// <summary>
  /// Formats a column reference expression.
  /// </summary>
  /// <param name="columnRefExpression">
  /// The column reference expression to format.
  /// </param>
  /// <returns>
  /// The formatted column reference expression.
  /// </returns>
  public string ColumnRefExpression(SqlColumnRefExpression columnRefExpression)
  {
    return Identifier(columnRefExpression.ColumnName);
  }

  /// <summary>
  /// Formats a common table expression.
  /// </summary>
  /// <param name="commonTableExpression">
  /// The common table expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted common table expression to.
  /// </param>
  public void CommonTableExpression(SqlCommonTableExpression commonTableExpression, ref List<string> lines)
  {
    string header = $"{Keyword(Keywords.WITH)} {Identifier(commonTableExpression.Name)} ";

    if (commonTableExpression.ColumnList is SqlIdentifierCollection columnList)
    {
      header += $"({string.Join(", ", columnList.Select(Identifier))}) ";
    }

    header += $"{Keyword(Keywords.AS)} (";

    List<string> cteLines = [];

    QueryExpression(commonTableExpression.QueryExpression, ref cteLines);

    lines.Add(header);
    lines.AddRange(cteLines.Select(line => $"{Indent()}{line}"));
    lines.Add(")");
  }

  /// <summary>
  /// Formats a comparison boolean expression.
  /// </summary>
  /// <param name="comparisonBooleanExpression">
  /// The comparison boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted comparison boolean expression to.
  /// </param>
  public void ComparisonBooleanExpression(SqlComparisonBooleanExpression comparisonBooleanExpression, ref List<string> lines)
  {
    List<string> leftExpressionLines = [];

    ScalarExpression(comparisonBooleanExpression.Left, ref leftExpressionLines);

    if (leftExpressionLines.Count > 1)
    {
      lines.Add("(");
      lines.AddRange(leftExpressionLines.Select(line => $"{Indent()}{line}"));
      lines.Add(")");
    }
    else
    {
      lines.Add(leftExpressionLines.First());
    }

    Utils.AppendToLast(lines, $" {ComparisonBooleanExpressionType(comparisonBooleanExpression.ComparisonOperator)} ");

    List<string> rightExpressionLines = [];

    ScalarExpression(comparisonBooleanExpression.Right, ref rightExpressionLines);

    if (rightExpressionLines.Count > 1)
    {
      lines.Add("(");
      lines.AddRange(rightExpressionLines.Select(line => $"{Indent()}{line}"));
      lines.Add(")");
    }
    else
    {
      Utils.AppendToLast(lines, rightExpressionLines.First());
    }
  }

  /// <summary>
  /// Formats a comparison type.
  /// </summary>
  /// <param name="comparisonType">
  /// The comparison type to format.
  /// </param>
  /// <returns>
  /// The formatted comparison type.
  /// </returns>
  public string ComparisonBooleanExpressionType(SqlComparisonBooleanExpressionType comparisonType)
  {
    return comparisonType switch
    {
      SqlComparisonBooleanExpressionType.Equals => "=",
      SqlComparisonBooleanExpressionType.GreaterThan => ">",
      SqlComparisonBooleanExpressionType.LessThan => "<",
      SqlComparisonBooleanExpressionType.GreaterThanOrEqual => ">=",
      SqlComparisonBooleanExpressionType.LessThanOrEqual => "<=",
      SqlComparisonBooleanExpressionType.NotEqual => "<>",
      _ => string.Empty,
    };
  }

  /// <summary>
  /// Formats a FROM clause.
  /// </summary>
  /// <param name="fromClause">
  /// The FROM clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted FROM clause to.
  /// </param>
  public void FromClause(SqlFromClause fromClause, ref List<string> lines)
  {
    lines.Add($"{Keyword(Keywords.FROM)} ");

    foreach (SqlTableExpression tableExpression in fromClause.TableExpressions)
    {
      TableExpression(tableExpression, ref lines);
    }
  }

  /// <summary>
  /// Formats a GROUP BY clause.
  /// </summary>
  /// <param name="groupByClause">
  /// The GROUP BY clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted GROUP BY clause to.
  /// </param>
  public void GroupByClause(SqlGroupByClause groupByClause, ref List<string> lines)
  {
    lines.Add(Keyword(Keywords.GROUP_BY));

    foreach (SqlGroupByItem groupByItem in groupByClause.Items)
    {
      GroupByItem(groupByItem, ref lines);
    }
  }

  /// <summary>
  /// Formats a GROUP BY item.
  /// </summary>
  /// <param name="groupByItem">
  /// The GROUP BY item to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted GROUP BY item to.
  /// </param>
  public void GroupByItem(SqlGroupByItem groupByItem, ref List<string> lines)
  {
    switch (groupByItem)
    {
      case SqlSimpleGroupByItem simpleGroupByItem:
        {
          SimpleGroupByItem(simpleGroupByItem, ref lines);

          break;
        }

      default:
        {
          Utils.Debug($"Unrecognized GROUP BY item type: {groupByItem.GetType().FullName}");
          lines.Add(groupByItem.Sql); // TODO

          break;
        }
    }
  }

  /// <summary>
  /// Formats a HAVING clause.
  /// </summary>
  /// <param name="havingClause">
  /// The HAVING clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted HAVING clause to.
  /// </param>
  public void HavingClause(SqlHavingClause havingClause, ref List<string> lines)
  {
    lines.Add(Keyword(Keywords.HAVING));

    List<string> booleanExpressionLines = [];

    BooleanExpression(havingClause.Expression, ref booleanExpressionLines);

    lines.AddRange(booleanExpressionLines.Select(line => $"{Indent()}{line}"));
  }

  /// <summary>
  /// Formats a SQL hint.
  /// </summary>
  /// <param name="hint">
  /// The SQL hint to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted SQL hint to.
  /// </param>
  public void Hint(SqlHint hint, ref List<string> lines)
  {
    switch (hint)
    {
      case SqlTableHint tableHint:
        {
          TableHint(tableHint, ref lines);

          break;
        }

      default:
        {
          Utils.Debug($"Unrecognized hint type: {hint.GetType().FullName}");
          lines.Add(hint.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats an identifier.
  /// </summary>
  /// <param name="identifier">
  /// The identifier to format.
  /// </param>
  /// <returns>
  /// The formatted identifier.
  /// </returns>
  public string Identifier(SqlIdentifier identifier)
  {
    return Identifier(identifier.Value);
  }

  /// <summary>
  /// Formats an identifier according to the configured style.
  /// </summary>
  /// <param name="identifier">
  /// The identifier to format.
  /// </param>
  /// <returns>
  /// The formatted identifier.
  /// </returns>
  public string Identifier(string identifier)
  {
    return Options.IdentifierStyle switch
    {
      TransactSQLFormatterOptions.IdentifierStyles.DoubleQuotes => $"\"{identifier}\"",
      TransactSQLFormatterOptions.IdentifierStyles.None => identifier,
      TransactSQLFormatterOptions.IdentifierStyles.SquareBrackets => $"[{identifier}]",
      _ => identifier,
    };
  }

  /// <summary>
  /// Generates an indentation string for the specified level.
  /// </summary>
  /// <param name="level">
  /// The indentation level. Default is 1.
  /// </param>
  /// <returns>
  /// The indentation string.
  /// </returns>
  public string Indent(int level = 1)
  {
    return string.Concat(Enumerable.Repeat(Options.IndentationString, level));
  }

  /// <summary>
  /// Formats a keyword according to the configured casing.
  /// </summary>
  /// <param name="keyword">
  /// The keyword to format.
  /// </param>
  /// <returns>
  /// The formatted keyword.
  /// </returns>
  public string Keyword(string keyword)
  {
    return Options.KeywordCase switch
    {
      TransactSQLFormatterOptions.KeywordCasing.Lowercase => keyword.ToLowerInvariant(),
      TransactSQLFormatterOptions.KeywordCasing.Uppercase => keyword.ToUpperInvariant(),
      _ => keyword,
    };
  }

  /// <summary>
  /// Formats a literal expression.
  /// </summary>
  /// <param name="literalExpression">
  /// The literal expression to format.
  /// </param>
  /// <returns>
  /// The formatted literal expression.
  /// </returns>
  public string LiteralExpression(SqlLiteralExpression literalExpression)
  {
    return literalExpression.Type switch
    {
      LiteralValueType.Default => Keyword(Keywords.DEFAULT),
      LiteralValueType.Identifier => Identifier(literalExpression.Value),
      LiteralValueType.Null => Keyword(Keywords.NULL),
      LiteralValueType.String => $"'{literalExpression.Value.Replace("'", "''")}'",
      LiteralValueType.UnicodeString => $"N'{literalExpression.Value.Replace("'", "''")}'",
      _ => literalExpression.Value,
    };
  }

  /// <summary>
  /// Formats a multipart identifier.
  /// </summary>
  /// <param name="multipartIdentifier">
  /// The multipart identifier to format.
  /// </param>
  /// <returns>
  /// The formatted multipart identifier.
  /// </returns>
  public string MultipartIdentifier(SqlMultipartIdentifier multipartIdentifier)
  {
    return string.Join(".", multipartIdentifier.Select(Identifier));
  }

  /// <summary>
  /// Formats an object identifier.
  /// </summary>
  /// <param name="objectIdentifier">
  /// The object identifier to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted object identifier to.
  /// </param>
  public void ObjectIdentifier(SqlObjectIdentifier objectIdentifier, ref List<string> lines)
  {
    IEnumerable<string> sections = new SqlIdentifier?[]
    {
      objectIdentifier.ServerName,
      objectIdentifier.DatabaseName,
      objectIdentifier.SchemaName,
      objectIdentifier.ObjectName
    }
      .Where(section => section != null && !string.IsNullOrWhiteSpace(section.Value))
      .Select(section => Identifier(section!));

    Utils.AppendToLast(lines, $"{string.Join(".", sections)}.");
  }

  /// <summary>
  /// Formats an OFFSET...FETCH clause.
  /// </summary>
  /// <param name="offsetFetchClause">
  /// The OFFSET...FETCH clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted OFFSET...FETCH clause to.
  /// </param>
  public void OffsetFetchClause(SqlOffsetFetchClause offsetFetchClause, ref List<string> lines)
  {
    List<string> expressionLines = [];

    ScalarExpression(offsetFetchClause.Offset, ref expressionLines);

    if (expressionLines.Count > 1)
    {
      lines.Add($"{Keyword(Keywords.OFFSET)} (");
      lines.AddRange(expressionLines.Select(line => $"{Indent()}{line}"));
      lines.Add($") {Keyword(Keywords.ROWS)}");
    }
    else
    {
      lines.Add($"{Keyword(Keywords.OFFSET)} {expressionLines.First()} {Keyword(Keywords.ROWS)}");
    }

    if (offsetFetchClause.Fetch is SqlScalarExpression scalarExpression)
    {
      List<string> fetchExpressionLines = [];

      ScalarExpression(scalarExpression, ref fetchExpressionLines);

      if (fetchExpressionLines.Count > 1)
      {
        lines.Add($"{Keyword(Keywords.FETCH_NEXT)} (");
        lines.AddRange(fetchExpressionLines.Select(line => $"{Indent()}{line}"));
        lines.Add($") {Keyword(Keywords.ROWS_ONLY)}");
      }
      else
      {
        lines.Add($"{Keyword(Keywords.FETCH_NEXT)} {fetchExpressionLines.First()} {Keyword(Keywords.ROWS_ONLY)}");
      }
    }
  }

  /// <summary>
  /// Formats an ORDER BY clause.
  /// </summary>
  /// <param name="orderByClause">
  /// The ORDER BY clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted ORDER BY clause to.
  /// </param>
  public void OrderByClause(SqlOrderByClause orderByClause, ref List<string> lines)
  {
    lines.Add(Keyword(Keywords.ORDER_BY));

    foreach (SqlOrderByItem orderByItem in orderByClause.Items)
    {
      OrderByItem(orderByItem, ref lines);
    }

    if (orderByClause.OffsetFetchClause is SqlOffsetFetchClause offsetFetchClause)
    {
      OffsetFetchClause(offsetFetchClause, ref lines);
    }
  }

  /// <summary>
  /// Formats an ORDER BY item.
  /// </summary>
  /// <param name="orderByItem">
  /// The ORDER BY item to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted ORDER BY item to.
  /// </param>
  public void OrderByItem(SqlOrderByItem orderByItem, ref List<string> lines)
  {
    List<string> expressionLines = [];

    ScalarExpression(orderByItem.Expression, ref expressionLines);

    lines.AddRange(expressionLines.Select(line => $"{Indent()}{line}"));
    Utils.AppendToLast(
      lines,
      orderByItem.SortOrder switch
      {
        SqlSortOrder.Ascending => $" {Keyword(Keywords.ASC)}",
        SqlSortOrder.Descending => $" {Keyword(Keywords.DESC)}",
        _ => string.Empty,
      });
  }

  /// <summary>
  /// Formats a parse result.
  /// </summary>
  /// <param name="parseResult">
  /// The parse result to format.
  /// </param>
  /// <returns>
  /// The formatted T-SQL script.
  /// </returns>
  public string ParseResult(ParseResult parseResult)
  {
    List<string> lines = [];

    foreach (SqlBatch batch in parseResult.Script.Batches)
    {
      Batch(batch, ref lines);
    }

    return string.Join(Environment.NewLine, lines);
  }

  /// <summary>
  /// Formats a query expression.
  /// </summary>
  /// <param name="queryExpression">
  /// The query expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted query expression to.
  /// </param>
  public void QueryExpression(SqlQueryExpression queryExpression, ref List<string> lines)
  {
    switch (queryExpression)
    {
      case SqlQuerySpecification querySpecification:
        {
          QuerySpecification(querySpecification, ref lines);

          break;
        }

      default:
        {
          Utils.Debug($"Unrecognized query expression type: {queryExpression.GetType().FullName}");
          lines.Add(queryExpression.Sql); // TODO

          break;
        }
    }
  }

  /// <summary>
  /// Formats a query specification.
  /// </summary>
  /// <param name="querySpecification">
  /// The query specification to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted query specification to.
  /// </param>
  public void QuerySpecification(SqlQuerySpecification querySpecification, ref List<string> lines)
  {
    if (querySpecification.SelectClause is SqlSelectClause selectClause)
    {
      lines.Add(Keyword(Keywords.SELECT));

      if (selectClause.IsDistinct)
      {
        Utils.AppendToLast(lines, $" {Keyword(Keywords.DISTINCT)}");
      }

      if (selectClause.Top is SqlTopSpecification topSpecification)
      {
        TopSpecification(topSpecification, ref lines);
      }

      for (int selectExpressionIndex = 0; selectExpressionIndex < selectClause.SelectExpressions.Count; selectExpressionIndex += 1)
      {
        SqlSelectExpression selectExpression = selectClause.SelectExpressions[selectExpressionIndex];

        SelectExpression(selectExpression, ref lines);

        if (selectExpressionIndex < selectClause.SelectExpressions.Count - 1)
        {
          Utils.AppendToLast(lines, ",");
        }
      }
    }

    if (querySpecification.IntoClause is SqlSelectIntoClause intoClause)
    {
      SelectIntoClause(intoClause, ref lines);
    }

    if (querySpecification.FromClause is SqlFromClause fromClause)
    {
      FromClause(fromClause, ref lines);
    }

    if (querySpecification.WhereClause is SqlWhereClause whereClause)
    {
      WhereClause(whereClause, ref lines);
    }

    if (querySpecification.GroupByClause is SqlGroupByClause groupByClause)
    {
      GroupByClause(groupByClause, ref lines);
    }

    if (querySpecification.HavingClause is SqlHavingClause havingClause)
    {
      HavingClause(havingClause, ref lines);
    }
  }

  /// <summary>
  /// Formats a query with clause.
  /// </summary>
  /// <param name="queryWithClause">
  /// The query with clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted query with clause to.
  /// </param>
  public void QueryWithClause(SqlQueryWithClause queryWithClause, ref List<string> lines)
  {
    foreach (SqlCommonTableExpression commonTableExpression in queryWithClause.CommonTableExpressions)
    {
      CommonTableExpression(commonTableExpression, ref lines);
    }
  }

  /// <summary>
  /// Formats a scalar expression.
  /// </summary>
  /// <param name="scalarExpression">
  /// The scalar expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted scalar expression to.
  /// </param>
  public void ScalarExpression(SqlScalarExpression scalarExpression, ref List<string> lines)
  {
    switch (scalarExpression)
    {
      case SqlColumnRefExpression columnRefExpression:
        {
          lines.Add(ColumnRefExpression(columnRefExpression));

          break;
        }

      case SqlLiteralExpression literalExpression:
        {
          lines.Add(LiteralExpression(literalExpression));

          break;
        }

      case SqlScalarRefExpression scalarRefExpression:
        {
          lines.Add(MultipartIdentifier(scalarRefExpression.MultipartIdentifier));

          break;
        }

      case SqlScalarSubQueryExpression scalarSubQueryExpression:
        {
          List<string> subqueryLines = [];

          QueryExpression(scalarSubQueryExpression.QueryExpression, ref subqueryLines);

          lines.Add("(");
          lines.AddRange(subqueryLines.Select(line => $"{Indent()}{line}"));
          lines.Add(")");

          break;
        }

      default:
        {
          Utils.Debug($"Unrecognized scalar expression type: {scalarExpression.GetType().FullName}");
          lines.Add(scalarExpression.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a select expression.
  /// </summary>
  /// <param name="selectExpression">
  /// The select expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted select expression to.
  /// </param>
  public void SelectExpression(SqlSelectExpression selectExpression, ref List<string> lines)
  {
    switch (selectExpression)
    {
      case SqlSelectScalarExpression selectScalarExpression:
        {
          SelectScalarExpression(selectScalarExpression, ref lines);

          break;
        }

      case SqlSelectStarExpression selectStarExpression:
        {
          SelectStarExpression(selectStarExpression, ref lines);

          break;
        }

      default:
        {
          Utils.Debug($"Unrecognized select expression type: {selectExpression.GetType().FullName}");
          lines.Add(selectExpression.Sql); // TODO

          break;
        }
    }
  }

  /// <summary>
  /// Formats a SELECT...INTO clause.
  /// </summary>
  /// <param name="selectIntoClause">
  /// The SELECT...INTO clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted SELECT...INTO clause to.
  /// </param>
  public void SelectIntoClause(SqlSelectIntoClause selectIntoClause, ref List<string> lines)
  {
    lines.Add($"{Keyword(Keywords.INTO)} ");

    ObjectIdentifier(selectIntoClause.IntoTarget, ref lines);
  }

  /// <summary>
  /// Formats a select scalar expression.
  /// </summary>
  /// <param name="selectScalarExpression">
  /// The select scalar expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted select scalar expression to.
  /// </param>
  public void SelectScalarExpression(SqlSelectScalarExpression selectScalarExpression, ref List<string> lines)
  {
    List<string> expressionLines = [];

    ScalarExpression(selectScalarExpression.Expression, ref expressionLines);

    lines.AddRange(expressionLines.Select(line => $"{Indent()}{line}"));

    if (selectScalarExpression.Alias is SqlIdentifier aliasIdentifier)
    {
      Utils.AppendToLast(lines, $" {Keyword(Keywords.AS)} {Identifier(aliasIdentifier)}");
    }
  }

  /// <summary>
  /// Formats a select specification.
  /// </summary>
  /// <param name="selectSpecification">
  /// The select specification to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted select specification to.
  /// </param>
  public void SelectSpecification(SqlSelectSpecification selectSpecification, ref List<string> lines)
  {
    QueryExpression(selectSpecification.QueryExpression, ref lines);

    if (selectSpecification.OrderByClause is SqlOrderByClause orderByClause)
    {
      OrderByClause(orderByClause, ref lines);
    }
  }

  /// <summary>
  /// Formats a * expression.
  /// </summary>
  /// <param name="selectStarExpression">
  /// The select star expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted select star expression to.
  /// </param>
  public void SelectStarExpression(SqlSelectStarExpression selectStarExpression, ref List<string> lines)
  {
    lines.Add(Indent());

    if (selectStarExpression.Qualifier is SqlObjectIdentifier objectIdentifier)
    {
      ObjectIdentifier(objectIdentifier, ref lines);
    }

    Utils.AppendToLast(lines, "*");
  }

  /// <summary>
  /// Formats a SELECT statement.
  /// </summary>
  /// <param name="selectStatement">
  /// The SELECT statement to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted SELECT statement to.
  /// </param>
  public void SelectStatement(SqlSelectStatement selectStatement, ref List<string> lines)
  {
    if (selectStatement.QueryWithClause is SqlQueryWithClause queryWithClause)
    {
      QueryWithClause(queryWithClause, ref lines);
    }

    if (selectStatement.SelectSpecification is SqlSelectSpecification selectSpecification)
    {
      SelectSpecification(selectSpecification, ref lines);
    }
  }

  /// <summary>
  /// Formats a simple GROUP BY item.
  /// </summary>
  /// <param name="simpleGroupByItem">
  /// The simple GROUP BY item to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted simple GROUP BY item to.
  /// </param>
  public void SimpleGroupByItem(SqlSimpleGroupByItem simpleGroupByItem, ref List<string> lines)
  {
    List<string> expressionLines = [];

    ScalarExpression(simpleGroupByItem.Expression, ref expressionLines);

    lines.AddRange(expressionLines.Select(line => $"{Indent()}{line}"));
  }

  /// <summary>
  /// Formats a generic SQL statement.
  /// </summary>
  /// <param name="statement">
  /// The SQL statement to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted statement to.
  /// </param>
  public void Statement(SqlStatement statement, ref List<string> lines)
  {
    switch (statement)
    {
      case SqlSelectStatement selectStatement:
        {
          SelectStatement(selectStatement, ref lines);

          break;
        }

      default:
        {
          Utils.Debug($"Unrecognized statement type: {statement.GetType().FullName}");
          lines.Add(statement.Sql); // TODO

          break;
        }
    }
  }

  /// <summary>
  /// Formats a table expression.
  /// </summary>
  /// <param name="tableExpression">
  /// The table expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted table expression to.
  /// </param>
  public void TableExpression(SqlTableExpression tableExpression, ref List<string> lines)
  {
    switch (tableExpression)
    {
      case SqlTableRefExpression tableRefExpression:
        {
          TableRefExpression(tableRefExpression, ref lines);

          break;
        }

      default:
        {
          Utils.Debug($"Unrecognized table expression type: {tableExpression.GetType().FullName}");
          lines.Add(tableExpression.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a table hint.
  /// </summary>
  /// <param name="tableHint">
  /// The table hint to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted table hint to.
  /// </param>
  public void TableHint(SqlTableHint tableHint, ref List<string> lines)
  {
    lines.Add(Keyword(tableHint.Type.ToString()));
  }

  /// <summary>
  /// Formats a table reference expression.
  /// </summary>
  /// <param name="tableRefExpression">
  /// The table reference expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted table reference expression to.
  /// </param>
  public void TableRefExpression(SqlTableRefExpression tableRefExpression, ref List<string> lines)
  {
    ObjectIdentifier(tableRefExpression.ObjectIdentifier, ref lines);

    if (tableRefExpression.Alias is SqlIdentifier aliasIdentifier)
    {
      lines.Add($"{Indent()}{Keyword(Keywords.AS)} {Identifier(aliasIdentifier)}");
    }

    if (tableRefExpression.Hints is SqlHintCollection hintCollection)
    {
      List<string> hintLines = [];

      foreach (SqlHint hint in hintCollection)
      {
        Hint(hint, ref hintLines);
      }

      if (hintLines.Count > 1)
      {
        lines.Add($"{Indent()}{Keyword(Keywords.WITH)} (");
        lines.AddRange(hintLines.Select(line => $"{Indent()}{line}"));
        lines.Add($"{Indent()})");
      }
      else
      {
        lines.Add($"{Indent()}{Keyword(Keywords.WITH)} ({hintLines.First()})");
      }
    }
  }

  /// <summary>
  /// Formats a TOP specification.
  /// </summary>
  /// <param name="topSpecification">
  /// The TOP specification to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted TOP specification to.
  /// </param>
  public void TopSpecification(SqlTopSpecification topSpecification, ref List<string> lines)
  {
    Utils.AppendToLast(lines, $" {Keyword(Keywords.TOP)} ");

    List<string> topValueLines = [];

    ScalarExpression(topSpecification.Value, ref topValueLines);

    if (topValueLines.Count > 1)
    {
      Utils.AppendToLast(lines, "(");
      lines.AddRange(topValueLines.Select(line => $"{Indent()}{line}"));
      lines.Add(")");
    }
    else
    {
      Utils.AppendToLast(lines, topValueLines.First());
    }

    if (topSpecification.IsPercent)
    {
      Utils.AppendToLast(lines, $" {Keyword(Keywords.PERCENT)}");
    }

    if (topSpecification.IsWithTies)
    {
      Utils.AppendToLast(lines, $" {Keyword(Keywords.WITH_TIES)}");
    }
  }

  /// <summary>
  /// Formats a WHERE clause.
  /// </summary>
  /// <param name="whereClause">
  /// The WHERE clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted WHERE clause to.
  /// </param>
  public void WhereClause(SqlWhereClause whereClause, ref List<string> lines)
  {
    lines.Add(Keyword(Keywords.WHERE));

    List<string> booleanExpressionLines = [];

    BooleanExpression(whereClause.Expression, ref booleanExpressionLines);

    lines.AddRange(booleanExpressionLines.Select(line => $"{Indent()}{line}"));
  }
}
