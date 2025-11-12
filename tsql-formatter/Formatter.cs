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
  /// Formats an aggregate function call expression.
  /// </summary>
  /// <param name="aggregateFunctionCallExpression">
  /// The aggregate function call expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted aggregate function call expression to.
  /// </param>
  public void AggregateFunctionCallExpression(SqlAggregateFunctionCallExpression aggregateFunctionCallExpression, ref List<string> lines)
  {
    Utils.AppendToLast(lines, $"{Keyword(aggregateFunctionCallExpression.FunctionName)}(");
    SetQuantifier(aggregateFunctionCallExpression.SetQuantifier, ref lines);

    if (aggregateFunctionCallExpression.IsStar)
    {
      Utils.AppendToLast(lines, "*");
    }
    else if (aggregateFunctionCallExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
    {
      for (int argumentIndex = 0; argumentIndex < scalarExpressionCollection.Count; argumentIndex += 1)
      {
        SqlScalarExpression scalarExpression = scalarExpressionCollection[argumentIndex];

        ScalarExpression(scalarExpression, ref lines);

        if (argumentIndex < scalarExpressionCollection.Count - 1)
        {
          Utils.AppendToLast(lines, ", ");
        }
      }
    }

    Utils.AppendToLast(lines, ")");
  }

  /// <summary>
  /// Formats an ALL/ANY comparison boolean expression.
  /// </summary>
  /// <param name="allAnyComparisonBooleanExpression">
  /// The ALL/ANY comparison boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted ALL/ANY comparison boolean expression to.
  /// </param>
  public void AllAnyComparisonBooleanExpression(SqlAllAnyComparisonBooleanExpression allAnyComparisonBooleanExpression, ref List<string> lines)
  {
    List<string> leftExpressionLines = [string.Empty];

    ScalarExpression(allAnyComparisonBooleanExpression.Left, ref leftExpressionLines);

    if (leftExpressionLines.Count > 1)
    {
      Utils.AppendToLast(lines, "(");
      lines.AddRange(IndentStrings(leftExpressionLines));
      lines.Add(")");
    }
    else
    {
      Utils.AppendToLast(lines, leftExpressionLines.First());
    }

    ComparisonBooleanExpressionType(allAnyComparisonBooleanExpression.ComparisonOperator, ref lines);
    Utils.AppendToLast(lines, $"{Keyword(allAnyComparisonBooleanExpression.ComparisonType)} ");

    List<string> rightExpressionLines = [string.Empty];

    QueryExpression(allAnyComparisonBooleanExpression.Right, ref rightExpressionLines);

    if (rightExpressionLines.Count > 1)
    {
      Utils.AppendToLast(lines, "(");
      lines.AddRange(IndentStrings(rightExpressionLines));
      lines.Add(")");
    }
    else
    {
      Utils.AppendToLast(lines, rightExpressionLines.First());
    }
  }

  /// <summary>
  /// Formats an AT TIME ZONE expression.
  /// </summary>
  /// <param name="atTimeZoneExpression">
  /// The AT TIME ZONE expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted AT TIME ZONE expression to.
  /// </param>
  public void AtTimeZoneExpression(SqlAtTimeZoneExpression atTimeZoneExpression, ref List<string> lines)
  {
    ScalarExpression(atTimeZoneExpression.DateValue, ref lines);
    Utils.AppendToLast(lines, $" {Keyword(Keywords.AT_TIME_ZONE)} ");
    ScalarExpression(atTimeZoneExpression.TimeZone, ref lines);
  }

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
  /// Formats a BETWEEN boolean expression.
  /// </summary>
  /// <param name="betweenBooleanExpression">
  /// The BETWEEN boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted BETWEEN boolean expression to.
  /// </param>
  public void BetweenBooleanExpression(SqlBetweenBooleanExpression betweenBooleanExpression, ref List<string> lines)
  {
    ScalarExpression(betweenBooleanExpression.TestExpression, ref lines);
    Utils.AppendToLast(lines, $" {Keyword(Keywords.BETWEEN)} ");
    ScalarExpression(betweenBooleanExpression.BeginExpression, ref lines);
    Utils.AppendToLast(lines, $" {Keyword(Keywords.AND)} ");
    ScalarExpression(betweenBooleanExpression.EndExpression, ref lines);
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
    List<string> leftExpressionLines = [string.Empty];

    BooleanExpression(binaryBooleanExpression.Left, ref leftExpressionLines);

    if (leftExpressionLines.Count > 1)
    {
      Utils.AppendToLast(lines, "(");
      lines.AddRange(IndentStrings(leftExpressionLines));
      lines.Add(")");
    }
    else
    {
      Utils.AppendToLast(lines, leftExpressionLines.First());
    }

    lines.Add(string.Empty);
    BooleanOperatorType(binaryBooleanExpression.Operator, ref lines);
    Utils.AppendToLast(lines, " ");

    List<string> rightExpressionLines = [string.Empty];

    BooleanExpression(binaryBooleanExpression.Right, ref rightExpressionLines);

    if (rightExpressionLines.Count > 1)
    {
      Utils.AppendToLast(lines, "(");
      lines.AddRange(IndentStrings(rightExpressionLines));
      lines.Add(")");
    }
    else
    {
      Utils.AppendToLast(lines, rightExpressionLines.First());
    }
  }

  /// <summary>
  /// Formats a binary query expression.
  /// </summary>
  /// <param name="binaryQueryExpression">
  /// The binary query expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted binary query expression to.
  /// </param>
  public void BinaryQueryExpression(SqlBinaryQueryExpression binaryQueryExpression, ref List<string> lines)
  {
    QueryExpression(binaryQueryExpression.Left, ref lines);
    lines.Add(string.Empty);
    BinaryQueryOperatorType(binaryQueryExpression.Operator, ref lines);
    lines.Add(string.Empty);
    QueryExpression(binaryQueryExpression.Right, ref lines);
  }

  /// <summary>
  /// Formats a binary query operator type.
  /// </summary>
  /// <param name="binaryQueryOperatorType">
  /// The binary query operator type to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted binary query operator type to.
  /// </param>
  public void BinaryQueryOperatorType(SqlBinaryQueryOperatorType binaryQueryOperatorType, ref List<string> lines)
  {
    string op = binaryQueryOperatorType switch
    {
      SqlBinaryQueryOperatorType.Except => Keyword(Keywords.EXCEPT),
      SqlBinaryQueryOperatorType.Intersect => Keyword(Keywords.INTERSECT),
      SqlBinaryQueryOperatorType.Union => Keyword(Keywords.UNION),
      SqlBinaryQueryOperatorType.UnionAll => Keyword(Keywords.UNION_ALL),
      _ => binaryQueryOperatorType.ToString(),
    };

    Utils.AppendToLast(lines, op);
  }

  /// <summary>
  /// Formats a binary scalar expression.
  /// </summary>
  /// <param name="binaryScalarExpression">
  /// The binary scalar expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted binary scalar expression to.
  /// </param>
  public void BinaryScalarExpression(SqlBinaryScalarExpression binaryScalarExpression, ref List<string> lines)
  {
    ScalarExpression(binaryScalarExpression.Left, ref lines);
    BinaryScalarOperatorType(binaryScalarExpression.Operator, ref lines);
    ScalarExpression(binaryScalarExpression.Right, ref lines);
  }

  /// <summary>
  /// Formats a binary scalar operator type.
  /// </summary>
  /// <param name="binaryScalarOperatorType">
  /// The binary scalar operator type to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted binary scalar operator type to.
  /// </param>
  public void BinaryScalarOperatorType(SqlBinaryScalarOperatorType binaryScalarOperatorType, ref List<string> lines)
  {
    string op = binaryScalarOperatorType switch
    {
      SqlBinaryScalarOperatorType.Add => "+",
      SqlBinaryScalarOperatorType.Assign => "=",
      SqlBinaryScalarOperatorType.BitwiseAnd => "&",
      SqlBinaryScalarOperatorType.BitwiseOr => "|",
      SqlBinaryScalarOperatorType.BitwiseXor => "^",
      SqlBinaryScalarOperatorType.Divide => "/",
      SqlBinaryScalarOperatorType.DoublePipe => "||",
      SqlBinaryScalarOperatorType.Equals => "=",
      SqlBinaryScalarOperatorType.GreaterThan => ">",
      SqlBinaryScalarOperatorType.GreaterThanOrEqual => ">=",
      SqlBinaryScalarOperatorType.LeftShift => "<<",
      SqlBinaryScalarOperatorType.LessThan => "<",
      SqlBinaryScalarOperatorType.LessThanOrEqual => "<=",
      SqlBinaryScalarOperatorType.Modulus => "%",
      SqlBinaryScalarOperatorType.Multiply => "*",
      SqlBinaryScalarOperatorType.NotEqualTo => "<>",
      SqlBinaryScalarOperatorType.NotGreaterThan => "<=",
      SqlBinaryScalarOperatorType.NotLessThan => ">=",
      SqlBinaryScalarOperatorType.RightShift => ">>",
      SqlBinaryScalarOperatorType.Subtract => "-",
      _ => binaryScalarOperatorType.ToString(),
    };
    string formatted = Operator(op);

    Utils.AppendToLast(lines, formatted);
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
      case SqlAllAnyComparisonBooleanExpression allAnyComparisonBooleanExpression:
        {
          AllAnyComparisonBooleanExpression(allAnyComparisonBooleanExpression, ref lines);

          break;
        }

      case SqlBetweenBooleanExpression betweenBooleanExpression:
        {
          BetweenBooleanExpression(betweenBooleanExpression, ref lines);

          break;
        }

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

      case SqlExistsBooleanExpression existsBooleanExpression:
        {
          ExistsBooleanExpression(existsBooleanExpression, ref lines);

          break;
        }

      case SqlFullTextBooleanExpression fullTextBooleanExpression:
        {
          FullTextBooleanExpression(fullTextBooleanExpression, ref lines);

          break;
        }

      case SqlInBooleanExpression inBooleanExpression:
        {
          InBooleanExpression(inBooleanExpression, ref lines);

          break;
        }

      case SqlIsNullBooleanExpression isNullBooleanExpression:
        {
          IsNullBooleanExpression(isNullBooleanExpression, ref lines);

          break;
        }

      case SqlLikeBooleanExpression likeBooleanExpression:
        {
          LikeBooleanExpression(likeBooleanExpression, ref lines);

          break;
        }


      case SqlNotBooleanExpression notBooleanExpression:
        {
          NotBooleanExpression(notBooleanExpression, ref lines);

          break;
        }

      case SqlUpdateBooleanExpression updateBooleanExpression:
        {
          UpdateBooleanExpression(updateBooleanExpression, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlBooleanExpression), booleanExpression.GetType().Name, booleanExpression.Sql);
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
  /// <param name="lines">
  /// The lines to append the formatted boolean operator type to.
  /// </param>
  public void BooleanOperatorType(SqlBooleanOperatorType booleanOperatorType, ref List<string> lines)
  {
    string op = booleanOperatorType switch
    {
      SqlBooleanOperatorType.And => Keyword(Keywords.AND),
      SqlBooleanOperatorType.Or => Keyword(Keywords.OR),
      _ => booleanOperatorType.ToString(),
    };

    Utils.AppendToLast(lines, op);
  }

  /// <summary>
  /// Formats a built-in scalar function call expression.
  /// </summary>
  /// <param name="builtinScalarFunctionCallExpression">
  /// The built-in scalar function call expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted built-in scalar function call expression to.
  /// </param>
  public void BuiltinScalarFunctionCallExpression(SqlBuiltinScalarFunctionCallExpression builtinScalarFunctionCallExpression, ref List<string> lines)
  {
    switch (builtinScalarFunctionCallExpression)
    {
      case SqlAggregateFunctionCallExpression aggregateFunctionCallExpression:
        {
          AggregateFunctionCallExpression(aggregateFunctionCallExpression, ref lines);

          break;
        }

      case SqlCastExpression castExpression:
        {
          CastExpression(castExpression, ref lines);

          break;
        }

      case SqlIdentityFunctionCallExpression identityFunctionCallExpression:
        {
          IdentityFunctionCallExpression(identityFunctionCallExpression, ref lines);

          break;
        }

      default:
        {
          Utils.AppendToLast(lines, $"{Keyword(builtinScalarFunctionCallExpression.FunctionName)}(");

          if (builtinScalarFunctionCallExpression.IsStar)
          {
            Utils.AppendToLast(lines, "*");
          }
          else if (builtinScalarFunctionCallExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
          {
            for (int argumentIndex = 0; argumentIndex < scalarExpressionCollection.Count; argumentIndex += 1)
            {
              SqlScalarExpression scalarExpression = scalarExpressionCollection[argumentIndex];

              ScalarExpression(scalarExpression, ref lines);

              if (argumentIndex < scalarExpressionCollection.Count - 1)
              {
                Utils.AppendToLast(lines, ", ");
              }
            }
          }

          Utils.AppendToLast(lines, ")");

          break;
        }
    }
  }

  /// <summary>
  /// Formats a CASE expression.
  /// </summary>
  /// <param name="caseExpression">
  /// The CASE expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted CASE expression to.
  /// </param>
  public void CaseExpression(SqlCaseExpression caseExpression, ref List<string> lines)
  {
    switch (caseExpression)
    {
      case SqlSearchedCaseExpression searchedCaseExpression:
        {
          SearchedCaseExpression(searchedCaseExpression, ref lines);

          break;
        }

      case SqlSimpleCaseExpression simpleCaseExpression:
        {
          SimpleCaseExpression(simpleCaseExpression, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlCaseExpression), caseExpression.GetType().Name, caseExpression.Sql);
          lines.Add(caseExpression.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a CAST expression.
  /// </summary>
  /// <param name="castExpression">
  /// The CAST expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted CAST expression to.
  /// </param>
  public void CastExpression(SqlCastExpression castExpression, ref List<string> lines)
  {
    switch (castExpression)
    {
      case SqlConvertExpression convertExpression:
        {
          ConvertExpression(convertExpression, ref lines);

          break;
        }

      default:
        {
          Utils.AppendToLast(lines, $"{Keyword(castExpression.FunctionName)}(");

          if (castExpression.IsStar)
          {
            Utils.AppendToLast(lines, "*");
          }
          else if (castExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
          {
            for (int argumentIndex = 0; argumentIndex < scalarExpressionCollection.Count; argumentIndex += 1)
            {
              SqlScalarExpression scalarExpression = scalarExpressionCollection[argumentIndex];

              ScalarExpression(scalarExpression, ref lines);

              if (argumentIndex < scalarExpressionCollection.Count - 1)
              {
                Utils.AppendToLast(lines, ", ");
              }
            }
          }

          Utils.AppendToLast(lines, $" {Keyword(Keywords.AS)} ");
          DataTypeSpecification(castExpression.DataTypeSpec, ref lines);
          Utils.AppendToLast(lines, ")");

          break;
        }
    }
  }

  /// <summary>
  /// Formats a COLLATE scalar expression.
  /// </summary>
  /// <param name="collateScalarExpression">
  /// The COLLATE scalar expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted COLLATE scalar expression to.
  /// </param>
  public void CollateScalarExpression(SqlCollateScalarExpression collateScalarExpression, ref List<string> lines)
  {
    ScalarExpression(collateScalarExpression.Expression, ref lines);
    Utils.AppendToLast(lines, $" {Keyword(Keywords.COLLATE)} {collateScalarExpression.Collation.Name.Value}");
  }

  /// <summary>
  /// Formats a column reference expression.
  /// </summary>
  /// <param name="columnRefExpression">
  /// The column reference expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted column reference expression to.
  /// </param>
  public void ColumnRefExpression(SqlColumnRefExpression columnRefExpression, ref List<string> lines)
  {
    string identifier = Identifier(columnRefExpression.ColumnName);

    Utils.AppendToLast(lines, identifier);
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
    string header = $"{Keyword(Keywords.WITH)} {Identifier(commonTableExpression.Name)}";

    if (commonTableExpression.ColumnList is SqlIdentifierCollection columnList)
    {
      header += $" ({string.Join(", ", columnList.Select(Identifier))}) ";
    }

    List<string> cteLines = [string.Empty];

    QueryExpression(commonTableExpression.QueryExpression, ref cteLines);
    lines.Add(header);
    lines.Add($"{Keyword(Keywords.AS)} (");
    lines.AddRange(IndentStrings(cteLines));
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
    List<string> leftExpressionLines = [string.Empty];

    ScalarExpression(comparisonBooleanExpression.Left, ref leftExpressionLines);

    if (leftExpressionLines.Count > 1)
    {
      Utils.AppendToLast(lines, "(");
      lines.AddRange(IndentStrings(leftExpressionLines));
      lines.Add(")");
    }
    else
    {
      Utils.AppendToLast(lines, leftExpressionLines.First());
    }

    ComparisonBooleanExpressionType(comparisonBooleanExpression.ComparisonOperator, ref lines);

    List<string> rightExpressionLines = [string.Empty];

    ScalarExpression(comparisonBooleanExpression.Right, ref rightExpressionLines);

    if (rightExpressionLines.Count > 1)
    {
      Utils.AppendToLast(lines, "(");
      lines.AddRange(IndentStrings(rightExpressionLines));
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
  /// <param name="lines">
  /// The lines to append the formatted comparison type to.
  /// </param>
  public void ComparisonBooleanExpressionType(SqlComparisonBooleanExpressionType comparisonType, ref List<string> lines)
  {
    string op = comparisonType switch
    {
      SqlComparisonBooleanExpressionType.Equals => "=",
      SqlComparisonBooleanExpressionType.GreaterThan => ">",
      SqlComparisonBooleanExpressionType.GreaterThanOrEqual => ">=",
      SqlComparisonBooleanExpressionType.IsDistinctFrom => Keyword(Keywords.IS_DISTINCT_FROM),
      SqlComparisonBooleanExpressionType.IsNotDistinctFrom => Keyword(Keywords.IS_NOT_DISTINCT_FROM),
      SqlComparisonBooleanExpressionType.LeftStarEqualJoin => "*=",
      SqlComparisonBooleanExpressionType.LessOrGreaterThan => "<>",
      SqlComparisonBooleanExpressionType.LessThan => "<",
      SqlComparisonBooleanExpressionType.LessThanOrEqual => "<=",
      SqlComparisonBooleanExpressionType.NotEqual => "<>",
      SqlComparisonBooleanExpressionType.NotGreaterThan => "<=",
      SqlComparisonBooleanExpressionType.NotLessThan => ">=",
      SqlComparisonBooleanExpressionType.RightStarEqualJoin => "=*",
      SqlComparisonBooleanExpressionType.ValueEqual => "=",
      _ => comparisonType.ToString(),
    };
    string formatted = Operator(op);

    Utils.AppendToLast(lines, formatted);
  }

  /// <summary>
  /// Formats a condition clause.
  /// </summary>
  /// <param name="conditionClause">
  /// The condition clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted condition clause to.
  /// </param>
  public void ConditionClause(SqlConditionClause conditionClause, ref List<string> lines)
  {
    switch (conditionClause)
    {
      case SqlHavingClause havingClause:
        {
          HavingClause(havingClause, ref lines);

          break;
        }

      case SqlWhereClause whereClause:
        {
          WhereClause(whereClause, ref lines);

          break;
        }

      default:
        {
          BooleanExpression(conditionClause.Expression, ref lines);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a CONVERT expression.
  /// </summary>
  /// <param name="convertExpression">
  /// The CONVERT expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted CONVERT expression to.
  /// </param>
  public void ConvertExpression(SqlConvertExpression convertExpression, ref List<string> lines)
  {
    Utils.AppendToLast(lines, $"{Keyword(convertExpression.FunctionName)}(");

    if (convertExpression.IsStar)
    {
      Utils.AppendToLast(lines, "*");
    }
    else
    {
      DataTypeSpecification(convertExpression.DataTypeSpec, ref lines);

      if (convertExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
      {
        Utils.AppendToLast(lines, ", ");

        for (int argumentIndex = 0; argumentIndex < scalarExpressionCollection.Count; argumentIndex += 1)
        {
          SqlScalarExpression scalarExpression = scalarExpressionCollection[argumentIndex];

          ScalarExpression(scalarExpression, ref lines);

          if (argumentIndex < scalarExpressionCollection.Count - 1)
          {
            Utils.AppendToLast(lines, ", ");
          }
        }
      }
    }

    Utils.AppendToLast(lines, ")");
  }

  /// <summary>
  /// Formats a CUBE GROUP BY item.
  /// </summary>
  /// <param name="cubeGroupByItem">
  /// The CUBE GROUP BY item to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted CUBE GROUP BY item to.
  /// </param>
  public void CubeGroupByItem(SqlCubeGroupByItem cubeGroupByItem, ref List<string> lines)
  {
    Utils.AppendToLast(lines, $"{Keyword(Keywords.CUBE)} (");

    for (int itemIndex = 0; itemIndex < cubeGroupByItem.Items.Count; itemIndex += 1)
    {
      SqlSimpleGroupByItem simpleGroupByItem = cubeGroupByItem.Items[itemIndex];

      SimpleGroupByItem(simpleGroupByItem, ref lines);

      if (itemIndex < cubeGroupByItem.Items.Count - 1)
      {
        Utils.AppendToLast(lines, ", ");
      }
    }

    Utils.AppendToLast(lines, ")");
  }

  /// <summary>
  /// Formats a data type.
  /// </summary>
  /// <param name="dataType">
  /// The data type to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted data type to.
  /// </param>
  public void DataType(SqlDataType dataType, ref List<string> lines)
  {
    Utils.AppendToLast(lines, Keyword(dataType.ObjectIdentifier.ObjectName.Value));
  }

  /// <summary>
  /// Formats a data type specification.
  /// </summary>
  /// <param name="dataTypeSpecification">
  /// The data type specification to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted data type specification to.
  /// </param>
  public void DataTypeSpecification(SqlDataTypeSpecification dataTypeSpecification, ref List<string> lines)
  {
    DataType(dataTypeSpecification.DataType, ref lines);

    if (dataTypeSpecification.IsMaximum || dataTypeSpecification.Argument1.HasValue)
    {
      Utils.AppendToLast(lines, "(");

      if (dataTypeSpecification.IsMaximum)
      {
        Utils.AppendToLast(lines, Keyword(Keywords.MAX));
      }
      else if (dataTypeSpecification.Argument1.HasValue)
      {
        Utils.AppendToLast(lines, $"{dataTypeSpecification.Argument1.Value}");

        if (dataTypeSpecification.Argument2.HasValue)
        {
          Utils.AppendToLast(lines, $", {dataTypeSpecification.Argument2.Value}");
        }
      }

      Utils.AppendToLast(lines, ")");
    }
  }

  /// <summary>
  /// Formats a DELETE specification.
  /// </summary>
  /// <param name="deleteSpecification">
  /// The DELETE specification to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted DELETE specification to.
  /// </param>
  public void DeleteSpecification(SqlDeleteSpecification deleteSpecification, ref List<string> lines)
  {
    lines.Add(Keyword(Keywords.DELETE));

    if (deleteSpecification.TopSpecification is SqlTopSpecification topSpecification)
    {
      Utils.AppendToLast(lines, " ");
      TopSpecification(topSpecification, ref lines);
    }

    if (deleteSpecification.OutputClause is SqlOutputClause outputClause)
    {
      lines.Add(string.Empty);
      OutputClause(outputClause, ref lines);
    }
    else if (deleteSpecification.OutputIntoClause is SqlOutputIntoClause outputIntoClause)
    {
      lines.Add(string.Empty);
      OutputIntoClause(outputIntoClause, ref lines);
    }

    if (deleteSpecification.Target is SqlTableExpression tableExpression)
    {
      lines.Add(string.Empty);
      TableExpression(tableExpression, ref lines);
    }

    if (deleteSpecification.FromClause is SqlFromClause fromClause)
    {
      lines.Add(string.Empty);
      FromClause(fromClause, ref lines);
    }

    if (deleteSpecification.WhereClause is SqlWhereClause whereClause)
    {
      lines.Add(string.Empty);
      WhereClause(whereClause, ref lines);
    }
  }

  /// <summary>
  /// Formats a DELETE statement.
  /// </summary>
  /// <param name="deleteStatement">
  /// The DELETE statement to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted DELETE statement to.
  /// </param>
  public void DeleteStatement(SqlDeleteStatement deleteStatement, ref List<string> lines)
  {
    if (deleteStatement.QueryWithClause is SqlQueryWithClause queryWithClause)
    {
      QueryWithClause(queryWithClause, ref lines);
    }

    if (deleteStatement.DeleteSpecification is SqlDeleteSpecification deleteSpecification)
    {
      DeleteSpecification(deleteSpecification, ref lines);
    }
  }

  /// <summary>
  /// Formats a DML statement.
  /// </summary>
  /// <param name="dmlStatement">
  /// The DML statement to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted DML statement to.
  /// </param>
  public void DmlStatement(SqlDmlStatement dmlStatement, ref List<string> lines)
  {
    switch (dmlStatement)
    {
      case SqlDeleteStatement deleteStatement:
        {
          DeleteStatement(deleteStatement, ref lines);

          break;
        }

      // TODO

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlDmlStatement), dmlStatement.GetType().Name, dmlStatement.Sql);
          lines.Add(dmlStatement.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats an EXISTS boolean expression.
  /// </summary>
  /// <param name="existsBooleanExpression">
  /// The EXISTS boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted EXISTS boolean expression to.
  /// </param>
  public void ExistsBooleanExpression(SqlExistsBooleanExpression existsBooleanExpression, ref List<string> lines)
  {
    QueryExpression(existsBooleanExpression.QueryExpression, ref lines);
  }

  /// <summary>
  /// Formats a FOR BROWSE clause.
  /// </summary>
  /// <param name="forBrowseClause">
  /// The FOR BROWSE clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted FOR BROWSE clause to.
  /// </param>
  public void ForBrowseClause(SqlForBrowseClause forBrowseClause, ref List<string> lines)
  {
    Utils.AppendToLast(lines, $"{Keyword(Keywords.FOR)} {Keyword(Keywords.BROWSE)}");
  }

  /// <summary>
  /// Formats a FOR clause.
  /// </summary>
  /// <param name="forClause">
  /// The FOR clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted FOR clause to.
  /// </param>
  public void ForClause(SqlForClause forClause, ref List<string> lines)
  {
    switch (forClause)
    {
      case SqlForBrowseClause forBrowseClause:
        {
          ForBrowseClause(forBrowseClause, ref lines);

          break;
        }

      case SqlForXmlClause forXmlClause:
        {
          ForXmlClause(forXmlClause, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlForClause), forClause.GetType().Name, forClause.Sql);
          lines.Add(forClause.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a FOR XML AUTO clause.
  /// </summary>
  /// <param name="forXmlAutoClause"></param>
  /// <param name="lines"></param>
  public void ForXmlAutoClause(SqlForXmlAutoClause forXmlAutoClause, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlForXmlAutoClause), forXmlAutoClause.Sql);
    lines.Add(forXmlAutoClause.Sql);
  }

  /// <summary>
  /// Formats a FOR XML clause.
  /// </summary>
  /// <param name="forXmlClause">
  /// The FOR XML clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted FOR XML clause to.
  /// </param>
  public void ForXmlClause(SqlForXmlClause forXmlClause, ref List<string> lines)
  {
    switch (forXmlClause)
    {
      case SqlForXmlAutoClause forXmlAutoClause:
        {
          ForXmlAutoClause(forXmlAutoClause, ref lines);

          break;
        }

      case SqlForXmlExplicitClause forXmlExplicitClause:
        {
          ForXmlExplicitClause(forXmlExplicitClause, ref lines);

          break;
        }

      case SqlForXmlPathClause forXmlPathClause:
        {
          ForXmlPathClause(forXmlPathClause, ref lines);

          break;
        }

      case SqlForXmlRawClause forXmlRawClause:
        {
          ForXmlRawClause(forXmlRawClause, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlForXmlClause), forXmlClause.GetType().Name, forXmlClause.Sql);
          lines.Add(forXmlClause.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a FOR XML EXPLICIT clause.
  /// </summary>
  /// <param name="forXmlExplicitClause">
  /// The FOR XML EXPLICIT clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted FOR XML EXPLICIT clause to.
  /// </param>
  public void ForXmlExplicitClause(SqlForXmlExplicitClause forXmlExplicitClause, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlForXmlExplicitClause), forXmlExplicitClause.Sql);
    lines.Add(forXmlExplicitClause.Sql);
  }

  /// <summary>
  /// Formats a FOR XML PATH clause.
  /// </summary>
  /// <param name="forXmlPathClause">
  /// The FOR XML PATH clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted FOR XML PATH clause to.
  /// </param>
  public void ForXmlPathClause(SqlForXmlPathClause forXmlPathClause, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlForXmlPathClause), forXmlPathClause.Sql);
    lines.Add(forXmlPathClause.Sql);
  }

  /// <summary>
  /// Formats a FOR XML RAW clause.
  /// </summary>
  /// <param name="forXmlRawClause">
  /// The FOR XML RAW clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted FOR XML RAW clause to.
  /// </param>
  public void ForXmlRawClause(SqlForXmlRawClause forXmlRawClause, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlForXmlRawClause), forXmlRawClause.Sql);
    lines.Add(forXmlRawClause.Sql);
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
    Utils.AppendToLast(lines, $"{Keyword(Keywords.FROM)} ");

    foreach (SqlTableExpression tableExpression in fromClause.TableExpressions)
    {
      TableExpression(tableExpression, ref lines);
    }
  }

  /// <summary>
  /// Formats a FULLTEXT boolean expression.
  /// </summary>
  /// <param name="fullTextBooleanExpression">
  /// The FULLTEXT boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted FULLTEXT boolean expression to.
  /// </param>
  public void FullTextBooleanExpression(SqlFullTextBooleanExpression fullTextBooleanExpression, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlFullTextBooleanExpression), fullTextBooleanExpression.Sql);
    lines.Add(fullTextBooleanExpression.Sql);
  }

  /// <summary>
  /// Formats a global scalar variable reference expression.
  /// </summary>
  /// <param name="globalScalarVariableRefExpression">
  /// The global scalar variable reference expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted global scalar variable reference expression to.
  /// </param>
  public void GlobalScalarVariableRefExpression(SqlGlobalScalarVariableRefExpression globalScalarVariableRefExpression, ref List<string> lines)
  {
    Utils.AppendToLast(lines, globalScalarVariableRefExpression.VariableName);
  }

  /// <summary>
  /// Formats a GRAND TOTAL GROUP BY item.
  /// </summary>
  /// <param name="grandTotalGroupByItem">
  /// The GRAND TOTAL GROUP BY item to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted GRAND TOTAL GROUP BY item to.
  /// </param>
  public void GrandTotalGroupByItem(SqlGrandTotalGroupByItem grandTotalGroupByItem, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlGrandTotalGroupByItem), grandTotalGroupByItem.Sql);
    lines.Add(grandTotalGroupByItem.Sql); // TODO
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
    List<string> groupByLines = [string.Empty];

    for (int itemIndex = 0; itemIndex < groupByClause.Items.Count; itemIndex += 1)
    {
      SqlGroupByItem groupByItem = groupByClause.Items[itemIndex];

      GroupByItem(groupByItem, ref groupByLines);

      if (itemIndex < groupByClause.Items.Count - 1)
      {
        Utils.AppendToLast(groupByLines, ",");
        groupByLines.Add(string.Empty);
      }
    }

    Utils.AppendToLast(lines, Keyword(Keywords.GROUP_BY));
    lines.AddRange(IndentStrings(groupByLines));
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
      case SqlGrandTotalGroupByItem grandTotalGroupByItem:
        {
          GrandTotalGroupByItem(grandTotalGroupByItem, ref lines);

          break;
        }

      case SqlGroupBySets groupBySets:
        {
          GroupBySets(groupBySets, ref lines);

          break;
        }

      case SqlGroupingSetItem groupingSetItem:
        {
          GroupingSetItem(groupingSetItem, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlGroupByItem), groupByItem.GetType().Name, groupByItem.Sql);
          lines.Add(groupByItem.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a GROUP BY sets item.
  /// </summary>
  /// <param name="groupBySets">
  /// The GROUP BY sets item to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted GROUP BY sets item to.
  /// </param>
  public void GroupBySets(SqlGroupBySets groupBySets, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlGroupBySets), groupBySets.Sql);
    lines.Add(groupBySets.Sql); // TODO
  }

  /// <summary>
  /// Formats a grouping set item.
  /// </summary>
  /// <param name="groupingSetItem">
  /// The grouping set item to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted grouping set item to.
  /// </param>
  public void GroupingSetItem(SqlGroupingSetItem groupingSetItem, ref List<string> lines)
  {
    switch (groupingSetItem)
    {
      case SqlCubeGroupByItem cubeGroupByItem:
        {
          CubeGroupByItem(cubeGroupByItem, ref lines);

          break;
        }

      case SqlRollupGroupByItem rollupGroupByItem:
        {
          RollupGroupByItem(rollupGroupByItem, ref lines);

          break;
        }

      case SqlSimpleGroupByItem simpleGroupByItem:
        {
          SimpleGroupByItem(simpleGroupByItem, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlGroupingSetItem), groupingSetItem.GetType().Name, groupingSetItem.Sql);
          lines.Add(groupingSetItem.Sql);

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
    List<string> booleanExpressionLines = [string.Empty];

    BooleanExpression(havingClause.Expression, ref booleanExpressionLines);
    Utils.AppendToLast(lines, Keyword(Keywords.HAVING));
    lines.AddRange(IndentStrings(booleanExpressionLines));
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
      case SqlIndexHint indexHint:
        {
          IndexHint(indexHint, ref lines);

          break;
        }

      case SqlTableHint tableHint:
        {
          TableHint(tableHint, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlHint), hint.GetType().Name, hint.Sql);
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
  /// Formats an IDENTITY function call expression.
  /// </summary>
  /// <param name="identityFunctionCallExpression">
  /// The IDENTITY function call expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted IDENTITY function call expression to.
  /// </param>
  public void IdentityFunctionCallExpression(SqlIdentityFunctionCallExpression identityFunctionCallExpression, ref List<string> lines)
  {
    Utils.AppendToLast(lines, $"{Keyword(identityFunctionCallExpression.FunctionName)}(");

    if (identityFunctionCallExpression.IsStar)
    {
      Utils.AppendToLast(lines, "*");
    }
    else if (identityFunctionCallExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
    {
      for (int argumentIndex = 0; argumentIndex < scalarExpressionCollection.Count; argumentIndex += 1)
      {
        SqlScalarExpression scalarExpression = scalarExpressionCollection[argumentIndex];

        ScalarExpression(scalarExpression, ref lines);

        if (argumentIndex < scalarExpressionCollection.Count - 1)
        {
          Utils.AppendToLast(lines, ", ");
        }
      }
    }

    Utils.AppendToLast(lines, ")");
  }

  /// <summary>
  /// Formats an IN boolean expression.
  /// </summary>
  /// <param name="inBooleanExpression">
  /// The IN boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted IN boolean expression to.
  /// </param>
  public void InBooleanExpression(SqlInBooleanExpression inBooleanExpression, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlInBooleanExpression), inBooleanExpression.Sql);
    lines.Add(inBooleanExpression.Sql); // TODO
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
  public string Indentation(int level = 1)
  {
    return string.Concat(Enumerable.Repeat(Options.IndentationString, level));
  }

  /// <summary>
  /// Indents multiple lines by the specified level.
  /// </summary>
  /// <param name="lines">
  /// The lines to indent.
  /// </param>
  /// <param name="level">
  /// The indentation level. Default is 1.
  /// </param>
  /// <returns></returns>
  public IEnumerable<string> IndentStrings(IEnumerable<string> lines, int level = 1)
  {
    string indentation = Indentation(level);

    return lines.Select(line => $"{indentation}{line}");
  }

  /// <summary>
  /// Formats an INDEX hint.
  /// </summary>
  /// <param name="indexHint">
  /// The INDEX hint to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted INDEX hint to.
  /// </param>
  public void IndexHint(SqlIndexHint indexHint, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlIndexHint), indexHint.Sql);
    lines.Add(indexHint.Sql); // TODO
  }

  /// <summary>
  /// Formats an IS NULL boolean expression.
  /// </summary>
  /// <param name="isNullBooleanExpression">
  /// The IS NULL boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted IS NULL boolean expression to.
  /// </param>
  public void IsNullBooleanExpression(SqlIsNullBooleanExpression isNullBooleanExpression, ref List<string> lines)
  {
    ScalarExpression(isNullBooleanExpression.Expression, ref lines);
    Utils.AppendToLast(lines, $" {(isNullBooleanExpression.HasNot ? Keyword(Keywords.IS_NOT_NULL) : Keyword(Keywords.IS_NULL))}");
  }

  /// <summary>
  /// Formats a JOIN operator type.
  /// </summary>
  /// <param name="joinOperatorType">
  /// The JOIN operator type to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted JOIN operator type to.
  /// </param>
  public void JoinOperatorType(SqlJoinOperatorType joinOperatorType, ref List<string> lines)
  {
    string op = joinOperatorType switch
    {
      SqlJoinOperatorType.CrossApply => Keyword(Keywords.CROSS_APPLY),
      SqlJoinOperatorType.CrossJoin => Keyword(Keywords.CROSS_JOIN),
      SqlJoinOperatorType.FullOuterJoin => Keyword(Keywords.FULL_OUTER_JOIN),
      SqlJoinOperatorType.InnerJoin => Keyword(Keywords.INNER_JOIN),
      SqlJoinOperatorType.LeftOuterJoin => Keyword(Keywords.LEFT_OUTER_JOIN),
      SqlJoinOperatorType.OuterApply => Keyword(Keywords.OUTER_APPLY),
      SqlJoinOperatorType.RightOuterJoin => Keyword(Keywords.RIGHT_OUTER_JOIN),
      _ => joinOperatorType.ToString(),
    };

    Utils.AppendToLast(lines, op);
  }

  /// <summary>
  /// Formats a JOIN table expression.
  /// </summary>
  /// <param name="joinTableExpression">
  /// The JOIN table expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted JOIN table expression to.
  /// </param>
  public void JoinTableExpression(SqlJoinTableExpression joinTableExpression, ref List<string> lines)
  {
    switch (joinTableExpression)
    {
      case SqlQualifiedJoinTableExpression qualifiedJoinTableExpression:
        {
          QualifiedJoinTableExpression(qualifiedJoinTableExpression, ref lines);

          break;
        }

      // TODO

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlJoinTableExpression), joinTableExpression.GetType().Name, joinTableExpression.Sql);
          lines.Add(joinTableExpression.Sql);

          break;
        }
    }
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
      TransactSQLFormatterOptions.KeywordCases.Lowercase => keyword.ToLowerInvariant(),
      TransactSQLFormatterOptions.KeywordCases.Uppercase => keyword.ToUpperInvariant(),
      _ => keyword,
    };
  }

  /// <summary>
  /// Formats a LIKE boolean expression.
  /// </summary>
  /// <param name="likeBooleanExpression">
  /// The LIKE boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted LIKE boolean expression to.
  /// </param>
  public void LikeBooleanExpression(SqlLikeBooleanExpression likeBooleanExpression, ref List<string> lines)
  {
    ScalarExpression(likeBooleanExpression.Expression, ref lines);

    if (likeBooleanExpression.HasNot)
    {
      Utils.AppendToLast(lines, $" {Keyword(Keywords.NOT)}");
    }

    Utils.AppendToLast(lines, $" {Keyword(Keywords.LIKE)} ");

    ScalarExpression(likeBooleanExpression.LikePattern, ref lines);

    if (likeBooleanExpression.EscapeClause is SqlScalarExpression scalarExpression)
    {
      Utils.AppendToLast(lines, $" {Keyword(Keywords.ESCAPE)} ");
      ScalarExpression(scalarExpression, ref lines);
    }
  }

  /// <summary>
  /// Formats a literal expression.
  /// </summary>
  /// <param name="literalExpression">
  /// The literal expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted literal expression to.
  /// </param>
  public void LiteralExpression(SqlLiteralExpression literalExpression, ref List<string> lines)
  {
    string expression = literalExpression.Type switch
    {
      LiteralValueType.Binary => $"0x{literalExpression.Value}",
      LiteralValueType.Default => Keyword(Keywords.DEFAULT),
      LiteralValueType.Identifier => Identifier(literalExpression.Value),
      LiteralValueType.Image => literalExpression.Value,
      LiteralValueType.Integer => literalExpression.Value,
      LiteralValueType.Money => literalExpression.Value,
      LiteralValueType.Null => Keyword(Keywords.NULL),
      LiteralValueType.Numeric => literalExpression.Value,
      LiteralValueType.Real => literalExpression.Value,
      LiteralValueType.String => $"'{literalExpression.Value.Replace("'", "''")}'",
      LiteralValueType.UnicodeString => $"N'{literalExpression.Value.Replace("'", "''")}'",
      _ => literalExpression.Value,
    };

    Utils.AppendToLast(lines, expression);
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
  /// Formats a NOT boolean expression.
  /// </summary>
  /// <param name="notBooleanExpression">
  /// The NOT boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted NOT boolean expression to.
  /// </param>
  public void NotBooleanExpression(SqlNotBooleanExpression notBooleanExpression, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlNotBooleanExpression), notBooleanExpression.Sql);
    lines.Add(notBooleanExpression.Sql); // TODO
  }

  /// <summary>
  /// Formats a NULL query expression.
  /// </summary>
  /// <param name="nullQueryExpression">
  /// The NULL query expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted NULL query expression to.
  /// </param>
  public void NullQueryExpression(SqlNullQueryExpression nullQueryExpression, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlNullQueryExpression), nullQueryExpression.Sql);
    lines.Add(nullQueryExpression.Sql); // TODO
  }

  /// <summary>
  /// Formats a NULL scalar expression.
  /// </summary>
  /// <param name="nullScalarExpression">
  /// The NULL scalar expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted NULL scalar expression to.
  /// </param>
  public void NullScalarExpression(SqlNullScalarExpression nullScalarExpression, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlNullScalarExpression), nullScalarExpression.Sql);
    lines.Add(nullScalarExpression.Sql); // TODO
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
      .Where(section => !string.IsNullOrWhiteSpace(section?.Value))
      .Select(section => Identifier(section!));

    Utils.AppendToLast(lines, $"{string.Join(".", sections)}");
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
    lines.Add($"{Keyword(Keywords.OFFSET)} ");
    ScalarExpression(offsetFetchClause.Offset, ref lines);
    Utils.AppendToLast(lines, $" {Keyword(Keywords.ROWS)}");

    if (offsetFetchClause.Fetch is SqlScalarExpression scalarExpression)
    {
      List<string> fetchExpressionLines = [string.Empty];

      ScalarExpression(scalarExpression, ref fetchExpressionLines);

      lines.Add($"{Keyword(Keywords.FETCH_NEXT)} ");
      Utils.AppendToLast(lines, fetchExpressionLines);
      Utils.AppendToLast(lines, $" {Keyword(Keywords.ROWS_ONLY)}");
    }
  }

  /// <summary>
  /// Formats an operator according to the configured spacing.
  /// </summary>
  /// <param name="op">
  /// The operator to format.
  /// </param>
  /// <returns>
  /// The formatted operator.
  /// </returns>
  public string Operator(string op)
  {
    if (string.IsNullOrWhiteSpace(op))
    {
      return op;
    }

    return Options.OperatorSpacing switch
    {
      TransactSQLFormatterOptions.OperatorSpacings.Dense => op,
      TransactSQLFormatterOptions.OperatorSpacings.SpaceAround => $" {op} ",
      _ => op,
    };
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
    List<string> expressionLines = [string.Empty];

    ScalarExpression(orderByItem.Expression, ref expressionLines);
    lines.AddRange(IndentStrings(expressionLines));
    SortOrder(orderByItem.SortOrder, ref lines);
  }

  /// <summary>
  /// Formats an OUTPUT clause.
  /// </summary>
  /// <param name="outputClause">
  /// The OUTPUT clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted OUTPUT clause to.
  /// </param>
  public void OutputClause(SqlOutputClause outputClause, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlOutputClause), outputClause.Sql);
    lines.Add(outputClause.Sql); // TODO
  }

  /// <summary>
  /// Formats an OUTPUT INTO clause.
  /// </summary>
  /// <param name="outputIntoClause">
  /// The OUTPUT INTO clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted OUTPUT INTO clause to.
  /// </param>
  public void OutputIntoClause(SqlOutputIntoClause outputIntoClause, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlOutputIntoClause), outputIntoClause.Sql);
    lines.Add(outputIntoClause.Sql); // TODO
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
  /// Formats a qualified JOIN table expression.
  /// </summary>
  /// <param name="qualifiedJoinTableExpression">
  /// The qualified JOIN table expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted qualified JOIN table expression to.
  /// </param>
  public void QualifiedJoinTableExpression(SqlQualifiedJoinTableExpression qualifiedJoinTableExpression, ref List<string> lines)
  {
    List<string> onClauseLines = [string.Empty];

    TableExpression(qualifiedJoinTableExpression.Left, ref lines);
    lines.Add(string.Empty);
    JoinOperatorType(qualifiedJoinTableExpression.JoinOperator, ref lines);
    Utils.AppendToLast(lines, " ");
    TableExpression(qualifiedJoinTableExpression.Right, ref lines);
    lines.Add($"{Indentation()}{Keyword(Keywords.ON)}");
    ConditionClause(qualifiedJoinTableExpression.OnClause, ref onClauseLines);
    lines.AddRange(IndentStrings(onClauseLines, 2));
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
      case SqlBinaryQueryExpression binaryQueryExpression:
        {
          BinaryQueryExpression(binaryQueryExpression, ref lines);

          break;
        }

      case SqlNullQueryExpression nullQueryExpression:
        {
          NullQueryExpression(nullQueryExpression, ref lines);

          break;
        }

      case SqlQuerySpecification querySpecification:
        {
          QuerySpecification(querySpecification, ref lines);

          break;
        }

      case SqlTableConstructorExpression tableConstructorExpression:
        {
          TableConstructorExpression(tableConstructorExpression, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlQueryExpression), queryExpression.GetType().Name, queryExpression.Sql);
          lines.Add(queryExpression.Sql);

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
      SelectClause(selectClause, ref lines);
    }

    if (querySpecification.IntoClause is SqlSelectIntoClause intoClause)
    {
      lines.Add(string.Empty);
      SelectIntoClause(intoClause, ref lines);
    }

    if (querySpecification.FromClause is SqlFromClause fromClause)
    {
      lines.Add(string.Empty);
      FromClause(fromClause, ref lines);
    }

    if (querySpecification.WhereClause is SqlWhereClause whereClause)
    {
      lines.Add(string.Empty);
      WhereClause(whereClause, ref lines);
    }

    if (querySpecification.GroupByClause is SqlGroupByClause groupByClause)
    {
      lines.Add(string.Empty);
      GroupByClause(groupByClause, ref lines);
    }

    if (querySpecification.HavingClause is SqlHavingClause havingClause)
    {
      lines.Add(string.Empty);
      HavingClause(havingClause, ref lines);
    }

    if (querySpecification.OrderByClause is SqlOrderByClause orderByClause)
    {
      lines.Add(string.Empty);
      OrderByClause(orderByClause, ref lines);
    }

    if (querySpecification.ForClause is SqlForClause forClause)
    {
      lines.Add(string.Empty);
      ForClause(forClause, ref lines);
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
  /// Formats a ROLLUP GROUP BY item.
  /// </summary>
  /// <param name="rollupGroupByItem">
  /// The ROLLUP GROUP BY item to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted ROLLUP GROUP BY item to.
  /// </param>
  public void RollupGroupByItem(SqlRollupGroupByItem rollupGroupByItem, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlRollupGroupByItem), rollupGroupByItem.Sql);
    lines.Add(rollupGroupByItem.Sql); // TODO
  }

  /// <summary>
  /// Formats a set quantifier.
  /// </summary>
  /// <param name="setQuantifier">
  /// The set quantifier to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted set quantifier to.
  /// </param>
  public void SetQuantifier(SqlSetQuantifier setQuantifier, ref List<string> lines)
  {
    string raw = setQuantifier switch
    {
      SqlSetQuantifier.All => Keyword(Keywords.ALL),
      SqlSetQuantifier.Distinct => Keyword(Keywords.DISTINCT),
      SqlSetQuantifier.None => string.Empty,
      _ => setQuantifier.ToString(),
    };
    string value = string.IsNullOrWhiteSpace(raw) ? string.Empty : $"{raw} ";

    Utils.AppendToLast(lines, value);
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
      case SqlAtTimeZoneExpression atTimeZoneExpression:
        {
          AtTimeZoneExpression(atTimeZoneExpression, ref lines);

          break;
        }

      case SqlBinaryScalarExpression binaryScalarExpression:
        {
          BinaryScalarExpression(binaryScalarExpression, ref lines);

          break;
        }

      case SqlCaseExpression caseExpression:
        {
          CaseExpression(caseExpression, ref lines);

          break;
        }

      case SqlCollateScalarExpression collateScalarExpression:
        {
          CollateScalarExpression(collateScalarExpression, ref lines);

          break;
        }

      case SqlGlobalScalarVariableRefExpression globalScalarVariableRefExpression:
        {
          GlobalScalarVariableRefExpression(globalScalarVariableRefExpression, ref lines);

          break;
        }

      case SqlLiteralExpression literalExpression:
        {
          LiteralExpression(literalExpression, ref lines);

          break;
        }

      case SqlNullScalarExpression nullScalarExpression:
        {
          NullScalarExpression(nullScalarExpression, ref lines);

          break;
        }

      case SqlScalarFunctionCallExpression scalarFunctionCallExpression:
        {
          ScalarFunctionCallExpression(scalarFunctionCallExpression, ref lines);

          break;
        }

      case SqlScalarRefExpression scalarRefExpression:
        {
          ScalarRefExpression(scalarRefExpression, ref lines);

          break;
        }

      case SqlScalarSubQueryExpression scalarSubQueryExpression:
        {
          ScalarSubQueryExpression(scalarSubQueryExpression, ref lines);

          break;
        }

      case SqlScalarVariableRefExpression scalarVariableRefExpression:
        {
          ScalarVariableRefExpression(scalarVariableRefExpression, ref lines);

          break;
        }

      case SqlSearchedWhenClause searchedWhenClause:
        {
          SearchedWhenClause(searchedWhenClause, ref lines);

          break;
        }

      case SqlUdtMemberExpression udtMemberExpression:
        {
          UdtMemberExpression(udtMemberExpression, ref lines);

          break;
        }

      case SqlUnaryScalarExpression unaryScalarExpression:
        {
          UnaryScalarExpression(unaryScalarExpression, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlScalarExpression), scalarExpression.GetType().Name, scalarExpression.Sql);
          Utils.AppendToLast(lines, scalarExpression.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a scalar function call expression.
  /// </summary>
  /// <param name="scalarFunctionCallExpression">
  /// The scalar function call expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted scalar function call expression to.
  /// </param>
  public void ScalarFunctionCallExpression(SqlScalarFunctionCallExpression scalarFunctionCallExpression, ref List<string> lines)
  {
    switch (scalarFunctionCallExpression)
    {
      case SqlBuiltinScalarFunctionCallExpression builtinScalarFunctionCallExpression:
        {
          BuiltinScalarFunctionCallExpression(builtinScalarFunctionCallExpression, ref lines);

          break;
        }

      case SqlUserDefinedScalarFunctionCallExpression userDefinedScalarFunctionCallExpression:
        {
          UserDefinedScalarFunctionCallExpression(userDefinedScalarFunctionCallExpression, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlScalarFunctionCallExpression), scalarFunctionCallExpression.GetType().Name, scalarFunctionCallExpression.Sql);
          lines.Add(scalarFunctionCallExpression.Sql);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a scalar reference expression.
  /// </summary>
  /// <param name="scalarRefExpression">
  /// The scalar reference expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted scalar reference expression to.
  /// </param>
  public void ScalarRefExpression(SqlScalarRefExpression scalarRefExpression, ref List<string> lines)
  {
    switch (scalarRefExpression)
    {
      case SqlColumnRefExpression columnRefExpression:
        {
          ColumnRefExpression(columnRefExpression, ref lines);

          break;
        }

      default:
        {
          string identifier = MultipartIdentifier(scalarRefExpression.MultipartIdentifier);

          Utils.AppendToLast(lines, identifier);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a scalar sub-query expression.
  /// </summary>
  /// <param name="scalarSubQueryExpression">
  /// The scalar sub-query expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted scalar sub-query expression to.
  /// </param>
  public void ScalarSubQueryExpression(SqlScalarSubQueryExpression scalarSubQueryExpression, ref List<string> lines)
  {
    List<string> subqueryLines = [string.Empty];

    QueryExpression(scalarSubQueryExpression.QueryExpression, ref subqueryLines);
    Utils.AppendToLast(lines, "(");
    lines.AddRange(IndentStrings(subqueryLines));
    lines.Add(")");
  }

  /// <summary>
  /// Formats a scalar variable reference expression.
  /// </summary>
  /// <param name="scalarVariableRefExpression">
  /// The scalar variable reference expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted scalar variable reference expression to.
  /// </param>
  public void ScalarVariableRefExpression(SqlScalarVariableRefExpression scalarVariableRefExpression, ref List<string> lines)
  {
    Utils.AppendToLast(lines, scalarVariableRefExpression.VariableName);
  }

  /// <summary>
  /// Formats a searched CASE expression.
  /// </summary>
  /// <param name="searchedCaseExpression">
  /// The searched CASE expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted searched CASE expression to.
  /// </param>
  public void SearchedCaseExpression(SqlSearchedCaseExpression searchedCaseExpression, ref List<string> lines)
  {
    Utils.AppendToLast(lines, Keyword(Keywords.CASE));

    foreach (SqlSearchedWhenClause searchedWhenClause in searchedCaseExpression.WhenClauses)
    {
      SearchedWhenClause(searchedWhenClause, ref lines);
    }

    if (searchedCaseExpression.ElseExpression is SqlScalarExpression elseExpression)
    {
      List<string> elseLines = [string.Empty];

      lines.Add(Keyword(Keywords.ELSE));
      ScalarExpression(elseExpression, ref elseLines);
      lines.AddRange(IndentStrings(elseLines));
    }

    lines.Add(Keyword(Keywords.END));
  }

  /// <summary>
  /// Formats a searched WHEN clause.
  /// </summary>
  /// <param name="searchedWhenClause">
  /// The searched WHEN clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted searched WHEN clause to.
  /// </param>
  public void SearchedWhenClause(SqlSearchedWhenClause searchedWhenClause, ref List<string> lines)
  {
    List<string> thenLines = [string.Empty];

    lines.Add($"{Keyword(Keywords.WHEN)} ");
    BooleanExpression(searchedWhenClause.WhenExpression, ref lines);
    Utils.AppendToLast(lines, $" {Keyword(Keywords.THEN)}");
    ScalarExpression(searchedWhenClause.ThenExpression, ref thenLines);
    lines.AddRange(IndentStrings(thenLines));
  }

  /// <summary>
  /// Formats a SELECT clause.
  /// </summary>
  /// <param name="selectClause">
  /// The select clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted select clause to.
  /// </param>
  public void SelectClause(SqlSelectClause selectClause, ref List<string> lines)
  {
    Utils.AppendToLast(lines, Keyword(Keywords.SELECT));

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

      case SqlSelectVariableAssignmentExpression selectVariableAssignmentExpression:
        {
          SelectVariableAssignmentExpression(selectVariableAssignmentExpression, ref lines);

          break;
        }

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlSelectExpression), selectExpression.GetType().Name, selectExpression.Sql);
          lines.Add(selectExpression.Sql);

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
    Utils.AppendToLast(lines, $"{Keyword(Keywords.INTO)} ");
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
    List<string> expressionLines = [string.Empty];

    ScalarExpression(selectScalarExpression.Expression, ref expressionLines);
    lines.AddRange(IndentStrings(expressionLines));

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
    lines.Add(string.Empty);
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
    lines.Add(Indentation());

    if (selectStarExpression.Qualifier is SqlObjectIdentifier objectIdentifier)
    {
      ObjectIdentifier(objectIdentifier, ref lines);
      Utils.AppendToLast(lines, ".");
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
  /// Formats a SELECT variable assignment expression.
  /// </summary>
  /// <param name="selectVariableAssignmentExpression">
  /// The SELECT variable assignment expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted SELECT variable assignment expression to.
  /// </param>
  public void SelectVariableAssignmentExpression(SqlSelectVariableAssignmentExpression selectVariableAssignmentExpression, ref List<string> lines)
  {
    List<string> variableLines = [selectVariableAssignmentExpression.VariableAssignment.Variable.VariableName];

    ComparisonBooleanExpressionType(SqlComparisonBooleanExpressionType.Equals, ref variableLines);
    ScalarExpression(selectVariableAssignmentExpression.VariableAssignment.Value, ref variableLines);
    lines.AddRange(IndentStrings(variableLines));
  }

  /// <summary>
  /// Formats a simple CASE expression.
  /// </summary>
  /// <param name="simpleCaseExpression">
  /// The simple CASE expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted simple CASE expression to.
  /// </param>
  public void SimpleCaseExpression(SqlSimpleCaseExpression simpleCaseExpression, ref List<string> lines)
  {
    Utils.AppendToLast(lines, $"{Keyword(Keywords.CASE)} ");
    ScalarExpression(simpleCaseExpression.TestExpression, ref lines);

    foreach (SqlSimpleWhenClause simpleWhenClause in simpleCaseExpression.WhenClauses)
    {
      SimpleWhenClause(simpleWhenClause, ref lines);
    }

    if (simpleCaseExpression.ElseExpression is SqlScalarExpression elseExpression)
    {
      List<string> elseLines = [string.Empty];

      lines.Add(Keyword(Keywords.ELSE));
      ScalarExpression(elseExpression, ref elseLines);
      lines.AddRange(IndentStrings(elseLines));
    }

    lines.Add(Keyword(Keywords.END));
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
    ScalarExpression(simpleGroupByItem.Expression, ref lines);
  }

  /// <summary>
  /// Formats a simple WHEN clause.
  /// </summary>
  /// <param name="simpleWhenClause">
  /// The simple WHEN clause to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted simple WHEN clause to.
  /// </param>
  public void SimpleWhenClause(SqlSimpleWhenClause simpleWhenClause, ref List<string> lines)
  {
    List<string> thenLines = [string.Empty];

    lines.Add($"{Keyword(Keywords.WHEN)} ");
    ScalarExpression(simpleWhenClause.WhenExpression, ref lines);
    Utils.AppendToLast(lines, $" {Keyword(Keywords.THEN)}");
    ScalarExpression(simpleWhenClause.ThenExpression, ref thenLines);
    lines.AddRange(IndentStrings(thenLines));
  }

  /// <summary>
  /// Formats a sort order.
  /// </summary>
  /// <param name="sortOrder">
  /// The sort order to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted sort order to.
  /// </param>
  public void SortOrder(SqlSortOrder sortOrder, ref List<string> lines)
  {
    string raw = sortOrder switch
    {
      SqlSortOrder.Ascending => Keyword(Keywords.ASC),
      SqlSortOrder.Descending => Keyword(Keywords.DESC),
      SqlSortOrder.None => string.Empty,
      _ => sortOrder.ToString(),
    };
    string value = string.IsNullOrWhiteSpace(raw) ? string.Empty : $" {raw}";

    Utils.AppendToLast(lines, value);
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
      case SqlDmlStatement dmlStatement:
        {
          DmlStatement(dmlStatement, ref lines);

          break;
        }

      case SqlSelectStatement selectStatement:
        {
          SelectStatement(selectStatement, ref lines);

          break;
        }

      // TODO

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlStatement), statement.GetType().Name, statement.Sql);
          lines.Add(statement.Sql);

          break;
        }
    }

    Utils.AppendToLast(lines, ";");

    // Add configured number of blank lines between statements.
    foreach (int _ in Enumerable.Range(0, Options.LinesBetweenStatements))
    {
      lines.Add(string.Empty);
    }
  }

  /// <summary>
  /// Formats a table constructor expression.
  /// </summary>
  /// <param name="tableConstructorExpression">
  /// The table constructor expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted table constructor expression to.
  /// </param>
  public void TableConstructorExpression(SqlTableConstructorExpression tableConstructorExpression, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlTableConstructorExpression), tableConstructorExpression.Sql);
    lines.Add(tableConstructorExpression.Sql); // TODO
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
      case SqlJoinTableExpression joinTableExpression:
        {
          JoinTableExpression(joinTableExpression, ref lines);

          break;
        }

      case SqlTableRefExpression tableRefExpression:
        {
          TableRefExpression(tableRefExpression, ref lines);

          break;
        }

      // TODO

      default:
        {
          Utils.Debug("Unrecognized {0}: {1} | SQL: {2}", nameof(SqlTableExpression), tableExpression.GetType().Name, tableExpression.Sql);
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
    lines.Add(string.Empty);
    TableHintType(tableHint.Type, ref lines);
  }

  /// <summary>
  /// Formats a table hint type.
  /// </summary>
  /// <param name="tableHintType">
  /// The table hint type to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted table hint type to.
  /// </param>
  public void TableHintType(SqlTableHintType tableHintType, ref List<string> lines)
  {
    string raw = tableHintType switch
    {
      SqlTableHintType.FastFirstRow => Keywords.FASTFIRSTROW,
      SqlTableHintType.ForceSeek => Keywords.FORCESEEK,
      SqlTableHintType.HoldLock => Keywords.HOLDLOCK,
      SqlTableHintType.KeepDefaults => Keywords.KEEPDEFAULTS,
      SqlTableHintType.KeepIdentity => Keywords.KEEPIDENTITY,
      SqlTableHintType.NoExpand => Keywords.NOEXPAND,
      SqlTableHintType.NoLock => Keywords.NOLOCK,
      SqlTableHintType.NoWait => Keywords.NOWAIT,
      SqlTableHintType.None => string.Empty,
      SqlTableHintType.PageLock => Keywords.PAGELOCK,
      SqlTableHintType.ReadCommitted => Keywords.READCOMMITTED,
      SqlTableHintType.ReadCommittedLock => Keywords.READCOMMITTEDLOCK,
      SqlTableHintType.ReadPast => Keywords.READPAST,
      SqlTableHintType.ReadUncommitted => Keywords.READUNCOMMITTED,
      SqlTableHintType.RepeatableRead => Keywords.REPEATABLEREAD,
      SqlTableHintType.Rowlock => Keywords.ROWLOCK,
      SqlTableHintType.Serializable => Keywords.SERIALIZABLE,
      SqlTableHintType.Snapshot => Keywords.SNAPSHOT,
      SqlTableHintType.SpatialWindowMaxCells => Keywords.SPATIAL_WINDOW_MAX_CELLS,
      SqlTableHintType.TabLock => Keywords.TABLOCK,
      SqlTableHintType.TabLockX => Keywords.TABLOCKX,
      SqlTableHintType.UpdateLock => Keywords.UPDATELOCK,
      SqlTableHintType.XLock => Keywords.XLOCK,
      _ => tableHintType.ToString(),
    };
    string value = Keyword(raw);

    Utils.AppendToLast(lines, value);
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
      lines.Add($"{Indentation()}{Keyword(Keywords.AS)} {Identifier(aliasIdentifier)}");
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
        lines.Add($"{Indentation()}{Keyword(Keywords.WITH)} (");
        lines.AddRange(IndentStrings(hintLines));
        lines.Add($"{Indentation()})");
      }
      else
      {
        lines.Add($"{Indentation()}{Keyword(Keywords.WITH)} ({hintLines.First()})");
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
    List<string> topValueLines = [string.Empty];

    Utils.AppendToLast(lines, $" {Keyword(Keywords.TOP)} ");
    ScalarExpression(topSpecification.Value, ref topValueLines);

    if (topValueLines.Count > 1)
    {
      Utils.AppendToLast(lines, "(");
      lines.AddRange(IndentStrings(topValueLines));
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
  /// Formats a UDT member expression.
  /// </summary>
  /// <param name="udtMemberExpression">
  /// The UDT member expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted UDT member expression to.
  /// </param>
  public void UdtMemberExpression(SqlUdtMemberExpression udtMemberExpression, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlUdtMemberExpression), udtMemberExpression.Sql);
    lines.Add(udtMemberExpression.Sql); // TODO
  }

  /// <summary>
  /// Formats a unary scalar expression.
  /// </summary>
  /// <param name="unaryScalarExpression">
  /// The unary scalar expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted unary scalar expression to.
  /// </param>
  public void UnaryScalarExpression(SqlUnaryScalarExpression unaryScalarExpression, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlUnaryScalarExpression), unaryScalarExpression.Sql);
    lines.Add(unaryScalarExpression.Sql); // TODO
  }

  /// <summary>
  /// Formats an UPDATE boolean expression.
  /// </summary>
  /// <param name="updateBooleanExpression">
  /// The UPDATE boolean expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted UPDATE boolean expression to.
  /// </param>
  public void UpdateBooleanExpression(SqlUpdateBooleanExpression updateBooleanExpression, ref List<string> lines)
  {
    Utils.Debug("Unimplemented {0}: SQL: {1}", nameof(SqlUpdateBooleanExpression), updateBooleanExpression.Sql);
    lines.Add(updateBooleanExpression.Sql); // TODO
  }

  /// <summary>
  /// Formats a user-defined scalar function call expression.
  /// </summary>
  /// <param name="userDefinedScalarFunctionCallExpression">
  /// The user-defined scalar function call expression to format.
  /// </param>
  /// <param name="lines">
  /// The lines to append the formatted user-defined scalar function call expression to.
  /// </param>
  public void UserDefinedScalarFunctionCallExpression(SqlUserDefinedScalarFunctionCallExpression userDefinedScalarFunctionCallExpression, ref List<string> lines)
  {
    ObjectIdentifier(userDefinedScalarFunctionCallExpression.ObjectIdentifier, ref lines);
    Utils.AppendToLast(lines, "(");

    for (int argumentIndex = 0; argumentIndex < userDefinedScalarFunctionCallExpression.Arguments.Count; argumentIndex += 1)
    {
      SqlScalarExpression scalarExpression = userDefinedScalarFunctionCallExpression.Arguments[argumentIndex];

      ScalarExpression(scalarExpression, ref lines);

      if (argumentIndex < userDefinedScalarFunctionCallExpression.Arguments.Count - 1)
      {
        Utils.AppendToLast(lines, ", ");
      }
    }

    Utils.AppendToLast(lines, ")");
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
    List<string> booleanExpressionLines = [string.Empty];

    Utils.AppendToLast(lines, Keyword(Keywords.WHERE));
    BooleanExpression(whereClause.Expression, ref booleanExpressionLines);
    lines.AddRange(IndentStrings(booleanExpressionLines));
  }
}
