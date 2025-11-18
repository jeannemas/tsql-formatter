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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted aggregate function call expression to.
  /// </param>
  private void AggregateFunctionCallExpression(SqlAggregateFunctionCallExpression aggregateFunctionCallExpression, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine($"{Keyword(aggregateFunctionCallExpression.FunctionName)}(");
    SetQuantifier(aggregateFunctionCallExpression.SetQuantifier, ref stringBuilder);

    if (aggregateFunctionCallExpression.IsStar)
    {
      stringBuilder.AppendToLastLine("*");
    }
    else if (aggregateFunctionCallExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
    {
      ScalarExpressionCollection(scalarExpressionCollection, ref stringBuilder);
    }

    stringBuilder.AppendToLastLine(")");
  }

  /// <summary>
  /// Formats an ALL/ANY comparison boolean expression.
  /// </summary>
  /// <param name="allAnyComparisonBooleanExpression">
  /// The ALL/ANY comparison boolean expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted ALL/ANY comparison boolean expression to.
  /// </param>
  private void AllAnyComparisonBooleanExpression(SqlAllAnyComparisonBooleanExpression allAnyComparisonBooleanExpression, ref StringBuilder stringBuilder)
  {
    StringBuilder leftExpression = new();
    StringBuilder rightExpression = new();

    ScalarExpression(allAnyComparisonBooleanExpression.Left, ref leftExpression);

    if (leftExpression.IsMultiLine)
    {
      stringBuilder
        .AppendToLastLine("(")
        .AddNewLines(IndentLines(leftExpression))
        .AddNewLine(")");
    }
    else
    {
      stringBuilder.AppendToLastLine(leftExpression);
    }

    ComparisonBooleanExpressionType(allAnyComparisonBooleanExpression.ComparisonOperator, ref stringBuilder);
    stringBuilder.AppendToLastLine($"{Keyword(allAnyComparisonBooleanExpression.ComparisonType)} ");
    QueryExpression(allAnyComparisonBooleanExpression.Right, ref rightExpression);

    if (rightExpression.IsMultiLine)
    {
      stringBuilder
        .AppendToLastLine("(")
        .AddNewLines(IndentLines(rightExpression))
        .AddNewLine(")");
    }
    else
    {
      stringBuilder.AppendToLastLine(rightExpression);
    }
  }

  /// <summary>
  /// Formats an AT TIME ZONE expression.
  /// </summary>
  /// <param name="atTimeZoneExpression">
  /// The AT TIME ZONE expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted AT TIME ZONE expression to.
  /// </param>
  private void AtTimeZoneExpression(SqlAtTimeZoneExpression atTimeZoneExpression, ref StringBuilder stringBuilder)
  {
    ScalarExpression(atTimeZoneExpression.DateValue, ref stringBuilder);
    stringBuilder.AppendToLastLine($" {Keyword(Keywords.AT_TIME_ZONE)} ");
    ScalarExpression(atTimeZoneExpression.TimeZone, ref stringBuilder);
  }

  /// <summary>
  /// Formats a SQL batch.
  /// </summary>
  /// <param name="batch">
  /// The SQL batch to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted SQL batch to.
  /// </param>
  private void Batch(SqlBatch batch, ref StringBuilder stringBuilder)
  {
    foreach (SqlStatement statement in batch.Statements)
    {
      Statement(statement, ref stringBuilder);
    }
  }

  /// <summary>
  /// Formats a BETWEEN boolean expression.
  /// </summary>
  /// <param name="betweenBooleanExpression">
  /// The BETWEEN boolean expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted BETWEEN boolean expression to.
  /// </param>
  private void BetweenBooleanExpression(SqlBetweenBooleanExpression betweenBooleanExpression, ref StringBuilder stringBuilder)
  {
    ScalarExpression(betweenBooleanExpression.TestExpression, ref stringBuilder);
    stringBuilder.AppendToLastLine($" {Keyword(Keywords.BETWEEN)} ");
    ScalarExpression(betweenBooleanExpression.BeginExpression, ref stringBuilder);
    stringBuilder.AppendToLastLine($" {Keyword(Keywords.AND)} ");
    ScalarExpression(betweenBooleanExpression.EndExpression, ref stringBuilder);
  }

  /// <summary>
  /// Formats a binary boolean expression.
  /// </summary>
  /// <param name="binaryBooleanExpression">
  /// The binary boolean expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted binary boolean expression to.
  /// </param>
  private void BinaryBooleanExpression(SqlBinaryBooleanExpression binaryBooleanExpression, ref StringBuilder stringBuilder)
  {
    StringBuilder leftExpression = new();
    StringBuilder rightExpression = new();

    BooleanExpression(binaryBooleanExpression.Left, ref leftExpression);

    if (leftExpression.IsMultiLine)
    {
      stringBuilder
        .AppendToLastLine("(")
        .AddNewLines(IndentLines(leftExpression))
        .AddNewLine(")");
    }
    else
    {
      stringBuilder.AppendToLastLine(leftExpression);
    }

    stringBuilder.AddNewLine();
    BooleanOperatorType(binaryBooleanExpression.Operator, ref stringBuilder);
    stringBuilder.AppendToLastLine(" ");
    BooleanExpression(binaryBooleanExpression.Right, ref rightExpression);

    if (rightExpression.IsMultiLine)
    {
      stringBuilder
        .AppendToLastLine("(")
        .AddNewLines(IndentLines(rightExpression))
        .AddNewLine(")");
    }
    else
    {
      stringBuilder.AppendToLastLine(rightExpression);
    }
  }

  /// <summary>
  /// Formats a binary query expression.
  /// </summary>
  /// <param name="binaryQueryExpression">
  /// The binary query expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted binary query expression to.
  /// </param>
  private void BinaryQueryExpression(SqlBinaryQueryExpression binaryQueryExpression, ref StringBuilder stringBuilder)
  {
    QueryExpression(binaryQueryExpression.Left, ref stringBuilder);
    stringBuilder.AddNewLine();
    BinaryQueryOperatorType(binaryQueryExpression.Operator, ref stringBuilder);
    stringBuilder.AddNewLine();
    QueryExpression(binaryQueryExpression.Right, ref stringBuilder);
  }

  /// <summary>
  /// Formats a binary query operator type.
  /// </summary>
  /// <param name="binaryQueryOperatorType">
  /// The binary query operator type to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted binary query operator type to.
  /// </param>
  private void BinaryQueryOperatorType(SqlBinaryQueryOperatorType binaryQueryOperatorType, ref StringBuilder stringBuilder)
  {
    string op = binaryQueryOperatorType switch
    {
      SqlBinaryQueryOperatorType.Except => Keyword(Keywords.EXCEPT),
      SqlBinaryQueryOperatorType.Intersect => Keyword(Keywords.INTERSECT),
      SqlBinaryQueryOperatorType.Union => Keyword(Keywords.UNION),
      SqlBinaryQueryOperatorType.UnionAll => Keyword(Keywords.UNION_ALL),
      _ => binaryQueryOperatorType.ToString(),
    };

    stringBuilder.AppendToLastLine(op);
  }

  /// <summary>
  /// Formats a binary scalar expression.
  /// </summary>
  /// <param name="binaryScalarExpression">
  /// The binary scalar expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted binary scalar expression to.
  /// </param>
  private void BinaryScalarExpression(SqlBinaryScalarExpression binaryScalarExpression, ref StringBuilder stringBuilder)
  {
    ScalarExpression(binaryScalarExpression.Left, ref stringBuilder);
    BinaryScalarOperatorType(binaryScalarExpression.Operator, ref stringBuilder);
    ScalarExpression(binaryScalarExpression.Right, ref stringBuilder);
  }

  /// <summary>
  /// Formats a binary scalar operator type.
  /// </summary>
  /// <param name="binaryScalarOperatorType">
  /// The binary scalar operator type to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted binary scalar operator type to.
  /// </param>
  private void BinaryScalarOperatorType(SqlBinaryScalarOperatorType binaryScalarOperatorType, ref StringBuilder stringBuilder)
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

    stringBuilder.AppendToLastLine(formatted);
  }

  /// <summary>
  /// Formats a boolean expression.
  /// </summary>
  /// <param name="booleanExpression">
  /// The boolean expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted boolean expression to.
  /// </param>
  private void BooleanExpression(SqlBooleanExpression booleanExpression, ref StringBuilder stringBuilder)
  {
    switch (booleanExpression)
    {
      case SqlAllAnyComparisonBooleanExpression allAnyComparisonBooleanExpression:
        {
          AllAnyComparisonBooleanExpression(allAnyComparisonBooleanExpression, ref stringBuilder);

          break;
        }

      case SqlBetweenBooleanExpression betweenBooleanExpression:
        {
          BetweenBooleanExpression(betweenBooleanExpression, ref stringBuilder);

          break;
        }

      case SqlBinaryBooleanExpression binaryBooleanExpression:
        {
          BinaryBooleanExpression(binaryBooleanExpression, ref stringBuilder);

          break;
        }

      case SqlComparisonBooleanExpression comparisonBooleanExpression:
        {
          ComparisonBooleanExpression(comparisonBooleanExpression, ref stringBuilder);

          break;
        }

      case SqlExistsBooleanExpression existsBooleanExpression:
        {
          ExistsBooleanExpression(existsBooleanExpression, ref stringBuilder);

          break;
        }

      case SqlFullTextBooleanExpression fullTextBooleanExpression:
        {
          FullTextBooleanExpression(fullTextBooleanExpression, ref stringBuilder);

          break;
        }

      case SqlInBooleanExpression inBooleanExpression:
        {
          InBooleanExpression(inBooleanExpression, ref stringBuilder);

          break;
        }

      case SqlIsNullBooleanExpression isNullBooleanExpression:
        {
          IsNullBooleanExpression(isNullBooleanExpression, ref stringBuilder);

          break;
        }

      case SqlLikeBooleanExpression likeBooleanExpression:
        {
          LikeBooleanExpression(likeBooleanExpression, ref stringBuilder);

          break;
        }


      case SqlNotBooleanExpression notBooleanExpression:
        {
          NotBooleanExpression(notBooleanExpression, ref stringBuilder);

          break;
        }

      case SqlUpdateBooleanExpression updateBooleanExpression:
        {
          UpdateBooleanExpression(updateBooleanExpression, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(booleanExpression, ref stringBuilder);

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted boolean operator type to.
  /// </param>
  private void BooleanOperatorType(SqlBooleanOperatorType booleanOperatorType, ref StringBuilder stringBuilder)
  {
    string op = booleanOperatorType switch
    {
      SqlBooleanOperatorType.And => Keyword(Keywords.AND),
      SqlBooleanOperatorType.Or => Keyword(Keywords.OR),
      _ => booleanOperatorType.ToString(),
    };

    stringBuilder.AppendToLastLine(op);
  }

  /// <summary>
  /// Formats a built-in scalar function call expression.
  /// </summary>
  /// <param name="builtinScalarFunctionCallExpression">
  /// The built-in scalar function call expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted built-in scalar function call expression to.
  /// </param>
  private void BuiltinScalarFunctionCallExpression(SqlBuiltinScalarFunctionCallExpression builtinScalarFunctionCallExpression, ref StringBuilder stringBuilder)
  {
    switch (builtinScalarFunctionCallExpression)
    {
      case SqlAggregateFunctionCallExpression aggregateFunctionCallExpression:
        {
          AggregateFunctionCallExpression(aggregateFunctionCallExpression, ref stringBuilder);

          break;
        }

      case SqlCastExpression castExpression:
        {
          CastExpression(castExpression, ref stringBuilder);

          break;
        }

      case SqlIdentityFunctionCallExpression identityFunctionCallExpression:
        {
          IdentityFunctionCallExpression(identityFunctionCallExpression, ref stringBuilder);

          break;
        }

      default:
        {
          stringBuilder.AppendToLastLine($"{Keyword(builtinScalarFunctionCallExpression.FunctionName)}(");

          if (builtinScalarFunctionCallExpression.IsStar)
          {
            stringBuilder.AppendToLastLine("*");
          }
          else if (builtinScalarFunctionCallExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
          {
            ScalarExpressionCollection(scalarExpressionCollection, ref stringBuilder);
          }

          stringBuilder.AppendToLastLine(")");

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted CASE expression to.
  /// </param>
  private void CaseExpression(SqlCaseExpression caseExpression, ref StringBuilder stringBuilder)
  {
    switch (caseExpression)
    {
      case SqlSearchedCaseExpression searchedCaseExpression:
        {
          SearchedCaseExpression(searchedCaseExpression, ref stringBuilder);

          break;
        }

      case SqlSimpleCaseExpression simpleCaseExpression:
        {
          SimpleCaseExpression(simpleCaseExpression, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(caseExpression, ref stringBuilder);

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted CAST expression to.
  /// </param>
  private void CastExpression(SqlCastExpression castExpression, ref StringBuilder stringBuilder)
  {
    switch (castExpression)
    {
      case SqlConvertExpression convertExpression:
        {
          ConvertExpression(convertExpression, ref stringBuilder);

          break;
        }

      default:
        {
          stringBuilder.AppendToLastLine($"{Keyword(castExpression.FunctionName)}(");

          if (castExpression.IsStar)
          {
            stringBuilder.AppendToLastLine("*");
          }
          else if (castExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
          {
            ScalarExpressionCollection(scalarExpressionCollection, ref stringBuilder);
          }

          stringBuilder.AppendToLastLine($" {Keyword(Keywords.AS)} ");
          DataTypeSpecification(castExpression.DataTypeSpec, ref stringBuilder);
          stringBuilder.AppendToLastLine(")");

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted COLLATE scalar expression to.
  /// </param>
  private void CollateScalarExpression(SqlCollateScalarExpression collateScalarExpression, ref StringBuilder stringBuilder)
  {
    ScalarExpression(collateScalarExpression.Expression, ref stringBuilder);
    stringBuilder.AppendToLastLine($" {Keyword(Keywords.COLLATE)} {collateScalarExpression.Collation.Name.Value}");
  }

  /// <summary>
  /// Formats a column reference expression.
  /// </summary>
  /// <param name="columnRefExpression">
  /// The column reference expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted column reference expression to.
  /// </param>
  private void ColumnRefExpression(SqlColumnRefExpression columnRefExpression, ref StringBuilder stringBuilder)
  {
    string identifier = Identifier(columnRefExpression.ColumnName);

    stringBuilder.AppendToLastLine(identifier);
  }

  /// <summary>
  /// Formats a common table expression.
  /// </summary>
  /// <param name="commonTableExpression">
  /// The common table expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted common table expression to.
  /// </param>
  private void CommonTableExpression(SqlCommonTableExpression commonTableExpression, ref StringBuilder stringBuilder)
  {
    string header = $"{Keyword(Keywords.WITH)} {Identifier(commonTableExpression.Name)}";

    if (commonTableExpression.ColumnList is SqlIdentifierCollection columnList)
    {
      header += $" ({string.Join(", ", columnList.Select(Identifier))}) ";
    }

    StringBuilder cte = new();

    QueryExpression(commonTableExpression.QueryExpression, ref cte);
    stringBuilder
      .AddNewLine(header)
      .AddNewLine($"{Keyword(Keywords.AS)} (")
      .AddNewLines(IndentLines(cte))
      .AddNewLine(")");
  }

  /// <summary>
  /// Formats a comparison boolean expression.
  /// </summary>
  /// <param name="comparisonBooleanExpression">
  /// The comparison boolean expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted comparison boolean expression to.
  /// </param>
  private void ComparisonBooleanExpression(SqlComparisonBooleanExpression comparisonBooleanExpression, ref StringBuilder stringBuilder)
  {
    StringBuilder leftExpression = new();
    StringBuilder rightExpression = new();

    ScalarExpression(comparisonBooleanExpression.Left, ref leftExpression);

    if (leftExpression.IsMultiLine)
    {
      stringBuilder
        .AppendToLastLine("(")
        .AddNewLines(IndentLines(leftExpression))
        .AddNewLine(")");
    }
    else
    {
      stringBuilder.AppendToLastLine(leftExpression);
    }

    ComparisonBooleanExpressionType(comparisonBooleanExpression.ComparisonOperator, ref stringBuilder);
    ScalarExpression(comparisonBooleanExpression.Right, ref rightExpression);

    if (rightExpression.IsMultiLine)
    {
      stringBuilder
        .AppendToLastLine("(")
        .AddNewLines(IndentLines(rightExpression))
        .AddNewLine(")");
    }
    else
    {
      stringBuilder.AppendToLastLine(rightExpression);
    }
  }

  /// <summary>
  /// Formats a comparison type.
  /// </summary>
  /// <param name="comparisonType">
  /// The comparison type to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted comparison type to.
  /// </param>
  private void ComparisonBooleanExpressionType(SqlComparisonBooleanExpressionType comparisonType, ref StringBuilder stringBuilder)
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

    stringBuilder.AppendToLastLine(formatted);
  }

  /// <summary>
  /// Formats a condition clause.
  /// </summary>
  /// <param name="conditionClause">
  /// The condition clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted condition clause to.
  /// </param>
  private void ConditionClause(SqlConditionClause conditionClause, ref StringBuilder stringBuilder)
  {
    switch (conditionClause)
    {
      case SqlHavingClause havingClause:
        {
          HavingClause(havingClause, ref stringBuilder);

          break;
        }

      case SqlWhereClause whereClause:
        {
          WhereClause(whereClause, ref stringBuilder);

          break;
        }

      default:
        {
          BooleanExpression(conditionClause.Expression, ref stringBuilder);

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted CONVERT expression to.
  /// </param>
  private void ConvertExpression(SqlConvertExpression convertExpression, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine($"{Keyword(convertExpression.FunctionName)}(");

    if (convertExpression.IsStar)
    {
      stringBuilder.AppendToLastLine("*");
    }
    else
    {
      DataTypeSpecification(convertExpression.DataTypeSpec, ref stringBuilder);

      if (convertExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
      {
        stringBuilder.AppendToLastLine(", ");
        ScalarExpressionCollection(scalarExpressionCollection, ref stringBuilder);
      }
    }

    stringBuilder.AppendToLastLine(")");
  }

  /// <summary>
  /// Formats a CUBE GROUP BY item.
  /// </summary>
  /// <param name="cubeGroupByItem">
  /// The CUBE GROUP BY item to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted CUBE GROUP BY item to.
  /// </param>
  private void CubeGroupByItem(SqlCubeGroupByItem cubeGroupByItem, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine($"{Keyword(Keywords.CUBE)} (");
    CubeRollupArgumentCollection(cubeGroupByItem.Items, ref stringBuilder);
    stringBuilder.AppendToLastLine(")");
  }

  /// <summary>
  /// Formats a CUBE/ROLLUP argument collection.
  /// </summary>
  /// <param name="cubeRollupArgumentCollection">
  /// The CUBE/ROLLUP argument collection to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted CUBE/ROLLUP argument collection to.
  /// </param>
  private void CubeRollupArgumentCollection(SqlCubeRollupArgumentCollection cubeRollupArgumentCollection, ref StringBuilder stringBuilder)
  {
    for (int index = 0; index < cubeRollupArgumentCollection.Count; index += 1)
    {
      SqlSimpleGroupByItem simpleGroupByItem = cubeRollupArgumentCollection[index];

      SimpleGroupByItem(simpleGroupByItem, ref stringBuilder);

      if (index < cubeRollupArgumentCollection.Count - 1)
      {
        stringBuilder.AppendToLastLine(", ");
      }
    }
  }

  /// <summary>
  /// Formats a data type.
  /// </summary>
  /// <param name="dataType">
  /// The data type to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted data type to.
  /// </param>
  private void DataType(SqlDataType dataType, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine(Keyword(dataType.ObjectIdentifier.ObjectName.Value));
  }

  /// <summary>
  /// Formats a data type specification.
  /// </summary>
  /// <param name="dataTypeSpecification">
  /// The data type specification to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted data type specification to.
  /// </param>
  private void DataTypeSpecification(SqlDataTypeSpecification dataTypeSpecification, ref StringBuilder stringBuilder)
  {
    DataType(dataTypeSpecification.DataType, ref stringBuilder);

    if (dataTypeSpecification.IsMaximum || dataTypeSpecification.Argument1.HasValue)
    {
      stringBuilder.AppendToLastLine("(");

      if (dataTypeSpecification.IsMaximum)
      {
        stringBuilder.AppendToLastLine(Keyword(Keywords.MAX));
      }
      else if (dataTypeSpecification.Argument1.HasValue)
      {
        stringBuilder.AppendToLastLine($"{dataTypeSpecification.Argument1.Value}");

        if (dataTypeSpecification.Argument2.HasValue)
        {
          stringBuilder.AppendToLastLine($", {dataTypeSpecification.Argument2.Value}");
        }
      }

      stringBuilder.AppendToLastLine(")");
    }
  }

  /// <summary>
  /// Formats a DELETE specification.
  /// </summary>
  /// <param name="deleteSpecification">
  /// The DELETE specification to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted DELETE specification to.
  /// </param>
  private void DeleteSpecification(SqlDeleteSpecification deleteSpecification, ref StringBuilder stringBuilder)
  {
    stringBuilder.AddNewLine(Keyword(Keywords.DELETE));

    if (deleteSpecification.TopSpecification is SqlTopSpecification topSpecification)
    {
      stringBuilder.AppendToLastLine(" ");
      TopSpecification(topSpecification, ref stringBuilder);
    }

    if (deleteSpecification.OutputClause is SqlOutputClause outputClause)
    {
      stringBuilder.AddNewLine();
      OutputClause(outputClause, ref stringBuilder);
    }
    else if (deleteSpecification.OutputIntoClause is SqlOutputIntoClause outputIntoClause)
    {
      stringBuilder.AddNewLine();
      OutputIntoClause(outputIntoClause, ref stringBuilder);
    }

    if (deleteSpecification.Target is SqlTableExpression tableExpression)
    {
      stringBuilder.AddNewLine();
      TableExpression(tableExpression, ref stringBuilder);
    }

    if (deleteSpecification.FromClause is SqlFromClause fromClause)
    {
      stringBuilder.AddNewLine();
      FromClause(fromClause, ref stringBuilder);
    }

    if (deleteSpecification.WhereClause is SqlWhereClause whereClause)
    {
      stringBuilder.AddNewLine();
      WhereClause(whereClause, ref stringBuilder);
    }
  }

  /// <summary>
  /// Formats a DELETE statement.
  /// </summary>
  /// <param name="deleteStatement">
  /// The DELETE statement to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted DELETE statement to.
  /// </param>
  private void DeleteStatement(SqlDeleteStatement deleteStatement, ref StringBuilder stringBuilder)
  {
    if (deleteStatement.QueryWithClause is SqlQueryWithClause queryWithClause)
    {
      QueryWithClause(queryWithClause, ref stringBuilder);
    }

    if (deleteStatement.DeleteSpecification is SqlDeleteSpecification deleteSpecification)
    {
      DeleteSpecification(deleteSpecification, ref stringBuilder);
    }
  }

  /// <summary>
  /// Formats a DML statement.
  /// </summary>
  /// <param name="dmlStatement">
  /// The DML statement to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted DML statement to.
  /// </param>
  private void DmlStatement(SqlDmlStatement dmlStatement, ref StringBuilder stringBuilder)
  {
    switch (dmlStatement)
    {
      case SqlDeleteStatement deleteStatement:
        {
          DeleteStatement(deleteStatement, ref stringBuilder);

          break;
        }

      // TODO

      default:
        {
          Utils.HandleNotImplemented(dmlStatement, ref stringBuilder);

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted EXISTS boolean expression to.
  /// </param>
  private void ExistsBooleanExpression(SqlExistsBooleanExpression existsBooleanExpression, ref StringBuilder stringBuilder)
  {
    QueryExpression(existsBooleanExpression.QueryExpression, ref stringBuilder);
  }

  /// <summary>
  /// Formats a FOR BROWSE clause.
  /// </summary>
  /// <param name="forBrowseClause">
  /// The FOR BROWSE clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted FOR BROWSE clause to.
  /// </param>
  private void ForBrowseClause(SqlForBrowseClause forBrowseClause, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine($"{Keyword(Keywords.FOR)} {Keyword(Keywords.BROWSE)}");
  }

  /// <summary>
  /// Formats a FOR clause.
  /// </summary>
  /// <param name="forClause">
  /// The FOR clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted FOR clause to.
  /// </param>
  private void ForClause(SqlForClause forClause, ref StringBuilder stringBuilder)
  {
    switch (forClause)
    {
      case SqlForBrowseClause forBrowseClause:
        {
          ForBrowseClause(forBrowseClause, ref stringBuilder);

          break;
        }

      case SqlForXmlClause forXmlClause:
        {
          ForXmlClause(forXmlClause, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(forClause, ref stringBuilder);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a FOR XML AUTO clause.
  /// </summary>
  /// <param name="forXmlAutoClause">
  /// The FOR XML AUTO clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted FOR XML AUTO clause to.
  /// </param>
  private void ForXmlAutoClause(SqlForXmlAutoClause forXmlAutoClause, ref StringBuilder stringBuilder)
  {
    stringBuilder.AddNewLine($"{Keyword(Keywords.FOR)} {Keyword(Keywords.XML)} {Keyword(Keywords.AUTO)}");
  }

  /// <summary>
  /// Formats a FOR XML clause.
  /// </summary>
  /// <param name="forXmlClause">
  /// The FOR XML clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted FOR XML clause to.
  /// </param>
  private void ForXmlClause(SqlForXmlClause forXmlClause, ref StringBuilder stringBuilder)
  {
    switch (forXmlClause)
    {
      case SqlForXmlAutoClause forXmlAutoClause:
        {
          ForXmlAutoClause(forXmlAutoClause, ref stringBuilder);

          break;
        }

      case SqlForXmlExplicitClause forXmlExplicitClause:
        {
          ForXmlExplicitClause(forXmlExplicitClause, ref stringBuilder);

          break;
        }

      case SqlForXmlPathClause forXmlPathClause:
        {
          ForXmlPathClause(forXmlPathClause, ref stringBuilder);

          break;
        }

      case SqlForXmlRawClause forXmlRawClause:
        {
          ForXmlRawClause(forXmlRawClause, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(forXmlClause, ref stringBuilder);

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted FOR XML EXPLICIT clause to.
  /// </param>
  private void ForXmlExplicitClause(SqlForXmlExplicitClause forXmlExplicitClause, ref StringBuilder stringBuilder)
  {
    stringBuilder.AddNewLine($"{Keyword(Keywords.FOR)} {Keyword(Keywords.XML)} {Keyword(Keywords.EXPLICIT)}");
  }

  /// <summary>
  /// Formats a FOR XML PATH clause.
  /// </summary>
  /// <param name="forXmlPathClause">
  /// The FOR XML PATH clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted FOR XML PATH clause to.
  /// </param>
  private void ForXmlPathClause(SqlForXmlPathClause forXmlPathClause, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(forXmlPathClause, ref stringBuilder);
  }

  /// <summary>
  /// Formats a FOR XML RAW clause.
  /// </summary>
  /// <param name="forXmlRawClause">
  /// The FOR XML RAW clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted FOR XML RAW clause to.
  /// </param>
  private void ForXmlRawClause(SqlForXmlRawClause forXmlRawClause, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(forXmlRawClause, ref stringBuilder);
  }

  /// <summary>
  /// Formats a FROM clause.
  /// </summary>
  /// <param name="fromClause">
  /// The FROM clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted FROM clause to.
  /// </param>
  private void FromClause(SqlFromClause fromClause, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine($"{Keyword(Keywords.FROM)} ");

    foreach (SqlTableExpression tableExpression in fromClause.TableExpressions)
    {
      TableExpression(tableExpression, ref stringBuilder);
    }
  }

  /// <summary>
  /// Formats a FULLTEXT boolean expression.
  /// </summary>
  /// <param name="fullTextBooleanExpression">
  /// The FULLTEXT boolean expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted FULLTEXT boolean expression to.
  /// </param>
  private void FullTextBooleanExpression(SqlFullTextBooleanExpression fullTextBooleanExpression, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(fullTextBooleanExpression, ref stringBuilder);
  }

  /// <summary>
  /// Formats a global scalar variable reference expression.
  /// </summary>
  /// <param name="globalScalarVariableRefExpression">
  /// The global scalar variable reference expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted global scalar variable reference expression to.
  /// </param>
  private void GlobalScalarVariableRefExpression(SqlGlobalScalarVariableRefExpression globalScalarVariableRefExpression, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine(globalScalarVariableRefExpression.VariableName);
  }

  /// <summary>
  /// Formats a GRAND TOTAL GROUP BY item.
  /// </summary>
  /// <param name="grandTotalGroupByItem">
  /// The GRAND TOTAL GROUP BY item to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted GRAND TOTAL GROUP BY item to.
  /// </param>
  private void GrandTotalGroupByItem(SqlGrandTotalGroupByItem grandTotalGroupByItem, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(grandTotalGroupByItem, ref stringBuilder);
  }

  /// <summary>
  /// Formats a GROUP BY clause.
  /// </summary>
  /// <param name="groupByClause">
  /// The GROUP BY clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted GROUP BY clause to.
  /// </param>
  private void GroupByClause(SqlGroupByClause groupByClause, ref StringBuilder stringBuilder)
  {
    StringBuilder groupBy = new();

    GroupByItemCollection(groupByClause.Items, ref groupBy);
    stringBuilder
      .AppendToLastLine(Keyword(Keywords.GROUP_BY))
      .AddNewLines(IndentLines(groupBy));
  }

  /// <summary>
  /// Formats a GROUP BY item.
  /// </summary>
  /// <param name="groupByItem">
  /// The GROUP BY item to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted GROUP BY item to.
  /// </param>
  private void GroupByItem(SqlGroupByItem groupByItem, ref StringBuilder stringBuilder)
  {
    switch (groupByItem)
    {
      case SqlGrandTotalGroupByItem grandTotalGroupByItem:
        {
          GrandTotalGroupByItem(grandTotalGroupByItem, ref stringBuilder);

          break;
        }

      case SqlGroupBySets groupBySets:
        {
          GroupBySets(groupBySets, ref stringBuilder);

          break;
        }

      case SqlGroupingSetItem groupingSetItem:
        {
          GroupingSetItem(groupingSetItem, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(groupByItem, ref stringBuilder);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a GROUP BY item collection.
  /// </summary>
  /// <param name="groupByItemCollection">
  /// The GROUP BY item collection to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted GROUP BY item collection to.
  /// </param>
  private void GroupByItemCollection(SqlGroupByItemCollection groupByItemCollection, ref StringBuilder stringBuilder)
  {
    for (int index = 0; index < groupByItemCollection.Count; index += 1)
    {
      SqlGroupByItem groupByItem = groupByItemCollection[index];

      GroupByItem(groupByItem, ref stringBuilder);

      if (index < groupByItemCollection.Count - 1)
      {
        stringBuilder
          .AppendToLastLine(",")
          .AddNewLine();
      }
    }
  }

  /// <summary>
  /// Formats a GROUP BY sets item.
  /// </summary>
  /// <param name="groupBySets">
  /// The GROUP BY sets item to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted GROUP BY sets item to.
  /// </param>
  private void GroupBySets(SqlGroupBySets groupBySets, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(groupBySets, ref stringBuilder);
  }

  /// <summary>
  /// Formats a grouping set item.
  /// </summary>
  /// <param name="groupingSetItem">
  /// The grouping set item to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted grouping set item to.
  /// </param>
  private void GroupingSetItem(SqlGroupingSetItem groupingSetItem, ref StringBuilder stringBuilder)
  {
    switch (groupingSetItem)
    {
      case SqlCubeGroupByItem cubeGroupByItem:
        {
          CubeGroupByItem(cubeGroupByItem, ref stringBuilder);

          break;
        }

      case SqlRollupGroupByItem rollupGroupByItem:
        {
          RollupGroupByItem(rollupGroupByItem, ref stringBuilder);

          break;
        }

      case SqlSimpleGroupByItem simpleGroupByItem:
        {
          SimpleGroupByItem(simpleGroupByItem, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(groupingSetItem, ref stringBuilder);

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted HAVING clause to.
  /// </param>
  private void HavingClause(SqlHavingClause havingClause, ref StringBuilder stringBuilder)
  {
    StringBuilder booleanExpression = new();

    BooleanExpression(havingClause.Expression, ref booleanExpression);
    stringBuilder
      .AppendToLastLine(Keyword(Keywords.HAVING))
      .AddNewLines(IndentLines(booleanExpression));
  }

  /// <summary>
  /// Formats a SQL hint.
  /// </summary>
  /// <param name="hint">
  /// The SQL hint to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted SQL hint to.
  /// </param>
  private void Hint(SqlHint hint, ref StringBuilder stringBuilder)
  {
    switch (hint)
    {
      case SqlIndexHint indexHint:
        {
          IndexHint(indexHint, ref stringBuilder);

          break;
        }

      case SqlTableHint tableHint:
        {
          TableHint(tableHint, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(hint, ref stringBuilder);

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
  private string Identifier(SqlIdentifier identifier)
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
  private string Identifier(string identifier)
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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted IDENTITY function call expression to.
  /// </param>
  private void IdentityFunctionCallExpression(SqlIdentityFunctionCallExpression identityFunctionCallExpression, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine($"{Keyword(identityFunctionCallExpression.FunctionName)}(");

    if (identityFunctionCallExpression.IsStar)
    {
      stringBuilder.AppendToLastLine("*");
    }
    else if (identityFunctionCallExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
    {
      ScalarExpressionCollection(scalarExpressionCollection, ref stringBuilder);
    }

    stringBuilder.AppendToLastLine(")");
  }

  /// <summary>
  /// Formats an IN boolean expression.
  /// </summary>
  /// <param name="inBooleanExpression">
  /// The IN boolean expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted IN boolean expression to.
  /// </param>
  private void InBooleanExpression(SqlInBooleanExpression inBooleanExpression, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(inBooleanExpression, ref stringBuilder);
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
  private string Indentation(int level = 1)
  {
    return string.Concat(Enumerable.Repeat(Options.IndentationString, level));
  }

  /// <summary>
  /// Indents multiple lines by the specified level.
  /// </summary>
  /// <param name="stringBuilder">
  /// The string builder containing the lines to indent.
  /// </param>
  /// <param name="level">
  /// The indentation level. Default is 1.
  /// </param>
  /// <returns>
  /// The indented lines.
  /// </returns>
  private IEnumerable<string> IndentLines(StringBuilder stringBuilder, int level = 1)
  {
    string indentation = Indentation(level);

    return stringBuilder.Lines.Select(line => $"{indentation}{line}");
  }

  /// <summary>
  /// Formats an INDEX hint.
  /// </summary>
  /// <param name="indexHint">
  /// The INDEX hint to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted INDEX hint to.
  /// </param>
  private void IndexHint(SqlIndexHint indexHint, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(indexHint, ref stringBuilder);
  }

  /// <summary>
  /// Formats an IS NULL boolean expression.
  /// </summary>
  /// <param name="isNullBooleanExpression">
  /// The IS NULL boolean expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted IS NULL boolean expression to.
  /// </param>
  private void IsNullBooleanExpression(SqlIsNullBooleanExpression isNullBooleanExpression, ref StringBuilder stringBuilder)
  {
    ScalarExpression(isNullBooleanExpression.Expression, ref stringBuilder);
    stringBuilder.AppendToLastLine($" {Keyword(isNullBooleanExpression.HasNot ? Keywords.IS_NOT_NULL : Keywords.IS_NULL)}");
  }

  /// <summary>
  /// Formats a JOIN operator type.
  /// </summary>
  /// <param name="joinOperatorType">
  /// The JOIN operator type to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted JOIN operator type to.
  /// </param>
  private void JoinOperatorType(SqlJoinOperatorType joinOperatorType, ref StringBuilder stringBuilder)
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

    stringBuilder.AppendToLastLine(op);
  }

  /// <summary>
  /// Formats a JOIN table expression.
  /// </summary>
  /// <param name="joinTableExpression">
  /// The JOIN table expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted JOIN table expression to.
  /// </param>
  private void JoinTableExpression(SqlJoinTableExpression joinTableExpression, ref StringBuilder stringBuilder)
  {
    switch (joinTableExpression)
    {
      case SqlQualifiedJoinTableExpression qualifiedJoinTableExpression:
        {
          QualifiedJoinTableExpression(qualifiedJoinTableExpression, ref stringBuilder);

          break;
        }

      // TODO

      default:
        {
          Utils.HandleNotImplemented(joinTableExpression, ref stringBuilder);

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
  private string Keyword(string keyword)
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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted LIKE boolean expression to.
  /// </param>
  private void LikeBooleanExpression(SqlLikeBooleanExpression likeBooleanExpression, ref StringBuilder stringBuilder)
  {
    ScalarExpression(likeBooleanExpression.Expression, ref stringBuilder);

    if (likeBooleanExpression.HasNot)
    {
      stringBuilder.AppendToLastLine($" {Keyword(Keywords.NOT)}");
    }

    stringBuilder.AppendToLastLine($" {Keyword(Keywords.LIKE)} ");

    ScalarExpression(likeBooleanExpression.LikePattern, ref stringBuilder);

    if (likeBooleanExpression.EscapeClause is SqlScalarExpression scalarExpression)
    {
      stringBuilder.AppendToLastLine($" {Keyword(Keywords.ESCAPE)} ");
      ScalarExpression(scalarExpression, ref stringBuilder);
    }
  }

  /// <summary>
  /// Formats a literal expression.
  /// </summary>
  /// <param name="literalExpression">
  /// The literal expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted literal expression to.
  /// </param>
  private void LiteralExpression(SqlLiteralExpression literalExpression, ref StringBuilder stringBuilder)
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

    stringBuilder.AppendToLastLine(expression);
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
  private string MultipartIdentifier(SqlMultipartIdentifier multipartIdentifier)
  {
    return string.Join(".", multipartIdentifier.Select(Identifier));
  }

  /// <summary>
  /// Formats a NOT boolean expression.
  /// </summary>
  /// <param name="notBooleanExpression">
  /// The NOT boolean expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted NOT boolean expression to.
  /// </param>
  private void NotBooleanExpression(SqlNotBooleanExpression notBooleanExpression, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(notBooleanExpression, ref stringBuilder);
  }

  /// <summary>
  /// Formats a NULL query expression.
  /// </summary>
  /// <param name="nullQueryExpression">
  /// The NULL query expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted NULL query expression to.
  /// </param>
  private void NullQueryExpression(SqlNullQueryExpression nullQueryExpression, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(nullQueryExpression, ref stringBuilder);
  }

  /// <summary>
  /// Formats a NULL scalar expression.
  /// </summary>
  /// <param name="nullScalarExpression">
  /// The NULL scalar expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted NULL scalar expression to.
  /// </param>
  private void NullScalarExpression(SqlNullScalarExpression nullScalarExpression, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(nullScalarExpression, ref stringBuilder);
  }

  /// <summary>
  /// Formats an object identifier.
  /// </summary>
  /// <param name="objectIdentifier">
  /// The object identifier to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted object identifier to.
  /// </param>
  private void ObjectIdentifier(SqlObjectIdentifier objectIdentifier, ref StringBuilder stringBuilder)
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

    stringBuilder.AppendToLastLine($"{string.Join(".", sections)}");
  }

  /// <summary>
  /// Formats an OFFSET...FETCH clause.
  /// </summary>
  /// <param name="offsetFetchClause">
  /// The OFFSET...FETCH clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted OFFSET...FETCH clause to.
  /// </param>
  private void OffsetFetchClause(SqlOffsetFetchClause offsetFetchClause, ref StringBuilder stringBuilder)
  {
    stringBuilder.AddNewLine($"{Keyword(Keywords.OFFSET)} ");
    ScalarExpression(offsetFetchClause.Offset, ref stringBuilder);
    stringBuilder.AppendToLastLine($" {Keyword(Keywords.ROWS)}");

    if (offsetFetchClause.Fetch is SqlScalarExpression scalarExpression)
    {
      StringBuilder fetchExpression = new();

      ScalarExpression(scalarExpression, ref fetchExpression);

      stringBuilder
        .AddNewLine($"{Keyword(Keywords.FETCH_NEXT)} ")
        .AppendToLastLine(fetchExpression)
        .AppendToLastLine($" {Keyword(Keywords.ROWS_ONLY)}");
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
  private string Operator(string op)
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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted ORDER BY clause to.
  /// </param>
  private void OrderByClause(SqlOrderByClause orderByClause, ref StringBuilder stringBuilder)
  {
    stringBuilder.AddNewLine(Keyword(Keywords.ORDER_BY));
    OrderByItemCollection(orderByClause.Items, ref stringBuilder);

    if (orderByClause.OffsetFetchClause is SqlOffsetFetchClause offsetFetchClause)
    {
      OffsetFetchClause(offsetFetchClause, ref stringBuilder);
    }
  }

  /// <summary>
  /// Formats an ORDER BY item.
  /// </summary>
  /// <param name="orderByItem">
  /// The ORDER BY item to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted ORDER BY item to.
  /// </param>
  private void OrderByItem(SqlOrderByItem orderByItem, ref StringBuilder stringBuilder)
  {
    StringBuilder expression = new();

    ScalarExpression(orderByItem.Expression, ref expression);
    stringBuilder.AddNewLines(IndentLines(expression));
    SortOrder(orderByItem.SortOrder, ref stringBuilder);
  }

  /// <summary>
  /// Formats an ORDER BY item collection.
  /// </summary>
  /// <param name="orderByItemCollection">
  /// The ORDER BY item collection to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted ORDER BY item collection to.
  /// </param>
  private void OrderByItemCollection(SqlOrderByItemCollection orderByItemCollection, ref StringBuilder stringBuilder)
  {
    for (int index = 0; index < orderByItemCollection.Count; index += 1)
    {
      SqlOrderByItem orderByItem = orderByItemCollection[index];

      OrderByItem(orderByItem, ref stringBuilder);

      if (index < orderByItemCollection.Count - 1)
      {
        stringBuilder.AppendToLastLine(",");
      }
    }
  }

  /// <summary>
  /// Formats an OUTPUT clause.
  /// </summary>
  /// <param name="outputClause">
  /// The OUTPUT clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted OUTPUT clause to.
  /// </param>
  private void OutputClause(SqlOutputClause outputClause, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(outputClause, ref stringBuilder);
  }

  /// <summary>
  /// Formats an OUTPUT INTO clause.
  /// </summary>
  /// <param name="outputIntoClause">
  /// The OUTPUT INTO clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted OUTPUT INTO clause to.
  /// </param>
  private void OutputIntoClause(SqlOutputIntoClause outputIntoClause, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(outputIntoClause, ref stringBuilder);
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
    StringBuilder stringBuilder = new();

    foreach (SqlBatch batch in parseResult.Script.Batches)
    {
      Batch(batch, ref stringBuilder);
    }

    return stringBuilder.ToString();
  }

  /// <summary>
  /// Formats a qualified JOIN table expression.
  /// </summary>
  /// <param name="qualifiedJoinTableExpression">
  /// The qualified JOIN table expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted qualified JOIN table expression to.
  /// </param>
  private void QualifiedJoinTableExpression(SqlQualifiedJoinTableExpression qualifiedJoinTableExpression, ref StringBuilder stringBuilder)
  {
    StringBuilder onClause = new();

    TableExpression(qualifiedJoinTableExpression.Left, ref stringBuilder);
    stringBuilder.AddNewLine();
    JoinOperatorType(qualifiedJoinTableExpression.JoinOperator, ref stringBuilder);
    stringBuilder.AppendToLastLine(" ");
    TableExpression(qualifiedJoinTableExpression.Right, ref stringBuilder);
    stringBuilder.AddNewLine($"{Indentation()}{Keyword(Keywords.ON)}");
    ConditionClause(qualifiedJoinTableExpression.OnClause, ref onClause);
    stringBuilder.AddNewLines(IndentLines(onClause, 2));
  }

  /// <summary>
  /// Formats a query expression.
  /// </summary>
  /// <param name="queryExpression">
  /// The query expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted query expression to.
  /// </param>
  private void QueryExpression(SqlQueryExpression queryExpression, ref StringBuilder stringBuilder)
  {
    switch (queryExpression)
    {
      case SqlBinaryQueryExpression binaryQueryExpression:
        {
          BinaryQueryExpression(binaryQueryExpression, ref stringBuilder);

          break;
        }

      case SqlNullQueryExpression nullQueryExpression:
        {
          NullQueryExpression(nullQueryExpression, ref stringBuilder);

          break;
        }

      case SqlQuerySpecification querySpecification:
        {
          QuerySpecification(querySpecification, ref stringBuilder);

          break;
        }

      case SqlTableConstructorExpression tableConstructorExpression:
        {
          TableConstructorExpression(tableConstructorExpression, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(queryExpression, ref stringBuilder);

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted query specification to.
  /// </param>
  private void QuerySpecification(SqlQuerySpecification querySpecification, ref StringBuilder stringBuilder)
  {
    if (querySpecification.SelectClause is SqlSelectClause selectClause)
    {
      SelectClause(selectClause, ref stringBuilder);
    }

    if (querySpecification.IntoClause is SqlSelectIntoClause intoClause)
    {
      stringBuilder.AddNewLine();
      SelectIntoClause(intoClause, ref stringBuilder);
    }

    if (querySpecification.FromClause is SqlFromClause fromClause)
    {
      stringBuilder.AddNewLine();
      FromClause(fromClause, ref stringBuilder);
    }

    if (querySpecification.WhereClause is SqlWhereClause whereClause)
    {
      stringBuilder.AddNewLine();
      WhereClause(whereClause, ref stringBuilder);
    }

    if (querySpecification.GroupByClause is SqlGroupByClause groupByClause)
    {
      stringBuilder.AddNewLine();
      GroupByClause(groupByClause, ref stringBuilder);
    }

    if (querySpecification.HavingClause is SqlHavingClause havingClause)
    {
      stringBuilder.AddNewLine();
      HavingClause(havingClause, ref stringBuilder);
    }

    if (querySpecification.OrderByClause is SqlOrderByClause orderByClause)
    {
      stringBuilder.AddNewLine();
      OrderByClause(orderByClause, ref stringBuilder);
    }

    if (querySpecification.ForClause is SqlForClause forClause)
    {
      stringBuilder.AddNewLine();
      ForClause(forClause, ref stringBuilder);
    }
  }

  /// <summary>
  /// Formats a query with clause.
  /// </summary>
  /// <param name="queryWithClause">
  /// The query with clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted query with clause to.
  /// </param>
  private void QueryWithClause(SqlQueryWithClause queryWithClause, ref StringBuilder stringBuilder)
  {
    foreach (SqlCommonTableExpression commonTableExpression in queryWithClause.CommonTableExpressions)
    {
      CommonTableExpression(commonTableExpression, ref stringBuilder);
    }
  }

  /// <summary>
  /// Formats a ROLLUP GROUP BY item.
  /// </summary>
  /// <param name="rollupGroupByItem">
  /// The ROLLUP GROUP BY item to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted ROLLUP GROUP BY item to.
  /// </param>
  private void RollupGroupByItem(SqlRollupGroupByItem rollupGroupByItem, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(rollupGroupByItem, ref stringBuilder);
  }

  /// <summary>
  /// Formats a set quantifier.
  /// </summary>
  /// <param name="setQuantifier">
  /// The set quantifier to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted set quantifier to.
  /// </param>
  private void SetQuantifier(SqlSetQuantifier setQuantifier, ref StringBuilder stringBuilder)
  {
    string raw = setQuantifier switch
    {
      SqlSetQuantifier.All => Keyword(Keywords.ALL),
      SqlSetQuantifier.Distinct => Keyword(Keywords.DISTINCT),
      SqlSetQuantifier.None => string.Empty,
      _ => setQuantifier.ToString(),
    };
    string value = string.IsNullOrWhiteSpace(raw) ? string.Empty : $"{raw} ";

    stringBuilder.AppendToLastLine(value);
  }

  /// <summary>
  /// Formats a scalar expression.
  /// </summary>
  /// <param name="scalarExpression">
  /// The scalar expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted scalar expression to.
  /// </param>
  private void ScalarExpression(SqlScalarExpression scalarExpression, ref StringBuilder stringBuilder)
  {
    switch (scalarExpression)
    {
      case SqlAtTimeZoneExpression atTimeZoneExpression:
        {
          AtTimeZoneExpression(atTimeZoneExpression, ref stringBuilder);

          break;
        }

      case SqlBinaryScalarExpression binaryScalarExpression:
        {
          BinaryScalarExpression(binaryScalarExpression, ref stringBuilder);

          break;
        }

      case SqlCaseExpression caseExpression:
        {
          CaseExpression(caseExpression, ref stringBuilder);

          break;
        }

      case SqlCollateScalarExpression collateScalarExpression:
        {
          CollateScalarExpression(collateScalarExpression, ref stringBuilder);

          break;
        }

      case SqlGlobalScalarVariableRefExpression globalScalarVariableRefExpression:
        {
          GlobalScalarVariableRefExpression(globalScalarVariableRefExpression, ref stringBuilder);

          break;
        }

      case SqlLiteralExpression literalExpression:
        {
          LiteralExpression(literalExpression, ref stringBuilder);

          break;
        }

      case SqlNullScalarExpression nullScalarExpression:
        {
          NullScalarExpression(nullScalarExpression, ref stringBuilder);

          break;
        }

      case SqlScalarFunctionCallExpression scalarFunctionCallExpression:
        {
          ScalarFunctionCallExpression(scalarFunctionCallExpression, ref stringBuilder);

          break;
        }

      case SqlScalarRefExpression scalarRefExpression:
        {
          ScalarRefExpression(scalarRefExpression, ref stringBuilder);

          break;
        }

      case SqlScalarSubQueryExpression scalarSubQueryExpression:
        {
          ScalarSubQueryExpression(scalarSubQueryExpression, ref stringBuilder);

          break;
        }

      case SqlScalarVariableRefExpression scalarVariableRefExpression:
        {
          ScalarVariableRefExpression(scalarVariableRefExpression, ref stringBuilder);

          break;
        }

      case SqlSearchedWhenClause searchedWhenClause:
        {
          SearchedWhenClause(searchedWhenClause, ref stringBuilder);

          break;
        }

      case SqlUdtMemberExpression udtMemberExpression:
        {
          UdtMemberExpression(udtMemberExpression, ref stringBuilder);

          break;
        }

      case SqlUnaryScalarExpression unaryScalarExpression:
        {
          UnaryScalarExpression(unaryScalarExpression, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(scalarExpression, ref stringBuilder);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a scalar expression collection.
  /// </summary>
  /// <param name="scalarExpressionCollection">
  /// The scalar expression collection to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted scalar expression collection to.
  /// </param>
  private void ScalarExpressionCollection(SqlScalarExpressionCollection scalarExpressionCollection, ref StringBuilder stringBuilder)
  {
    for (int index = 0; index < scalarExpressionCollection.Count; index += 1)
    {
      SqlScalarExpression scalarExpression = scalarExpressionCollection[index];

      ScalarExpression(scalarExpression, ref stringBuilder);

      if (index < scalarExpressionCollection.Count - 1)
      {
        stringBuilder.AppendToLastLine(", ");
      }
    }
  }

  /// <summary>
  /// Formats a scalar function call expression.
  /// </summary>
  /// <param name="scalarFunctionCallExpression">
  /// The scalar function call expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted scalar function call expression to.
  /// </param>
  private void ScalarFunctionCallExpression(SqlScalarFunctionCallExpression scalarFunctionCallExpression, ref StringBuilder stringBuilder)
  {
    switch (scalarFunctionCallExpression)
    {
      case SqlBuiltinScalarFunctionCallExpression builtinScalarFunctionCallExpression:
        {
          BuiltinScalarFunctionCallExpression(builtinScalarFunctionCallExpression, ref stringBuilder);

          break;
        }

      case SqlUserDefinedScalarFunctionCallExpression userDefinedScalarFunctionCallExpression:
        {
          UserDefinedScalarFunctionCallExpression(userDefinedScalarFunctionCallExpression, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(scalarFunctionCallExpression, ref stringBuilder);

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted scalar reference expression to.
  /// </param>
  private void ScalarRefExpression(SqlScalarRefExpression scalarRefExpression, ref StringBuilder stringBuilder)
  {
    switch (scalarRefExpression)
    {
      case SqlColumnRefExpression columnRefExpression:
        {
          ColumnRefExpression(columnRefExpression, ref stringBuilder);

          break;
        }

      default:
        {
          string identifier = MultipartIdentifier(scalarRefExpression.MultipartIdentifier);

          stringBuilder.AppendToLastLine(identifier);

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted scalar sub-query expression to.
  /// </param>
  private void ScalarSubQueryExpression(SqlScalarSubQueryExpression scalarSubQueryExpression, ref StringBuilder stringBuilder)
  {
    StringBuilder subquery = new();

    QueryExpression(scalarSubQueryExpression.QueryExpression, ref subquery);
    stringBuilder
      .AppendToLastLine("(")
      .AddNewLines(IndentLines(subquery))
      .AddNewLine(")");
  }

  /// <summary>
  /// Formats a scalar variable reference expression.
  /// </summary>
  /// <param name="scalarVariableRefExpression">
  /// The scalar variable reference expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted scalar variable reference expression to.
  /// </param>
  private void ScalarVariableRefExpression(SqlScalarVariableRefExpression scalarVariableRefExpression, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine(scalarVariableRefExpression.VariableName);
  }

  /// <summary>
  /// Formats a searched CASE expression.
  /// </summary>
  /// <param name="searchedCaseExpression">
  /// The searched CASE expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted searched CASE expression to.
  /// </param>
  private void SearchedCaseExpression(SqlSearchedCaseExpression searchedCaseExpression, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine(Keyword(Keywords.CASE));

    foreach (SqlSearchedWhenClause searchedWhenClause in searchedCaseExpression.WhenClauses)
    {
      SearchedWhenClause(searchedWhenClause, ref stringBuilder);
    }

    if (searchedCaseExpression.ElseExpression is SqlScalarExpression elseExpression)
    {
      StringBuilder _else = new();

      stringBuilder.AddNewLine(Keyword(Keywords.ELSE));
      ScalarExpression(elseExpression, ref _else);
      stringBuilder.AddNewLines(IndentLines(_else));
    }

    stringBuilder.AddNewLine(Keyword(Keywords.END));
  }

  /// <summary>
  /// Formats a searched WHEN clause.
  /// </summary>
  /// <param name="searchedWhenClause">
  /// The searched WHEN clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted searched WHEN clause to.
  /// </param>
  private void SearchedWhenClause(SqlSearchedWhenClause searchedWhenClause, ref StringBuilder stringBuilder)
  {
    StringBuilder then = new();

    stringBuilder.AddNewLine($"{Keyword(Keywords.WHEN)} ");
    BooleanExpression(searchedWhenClause.WhenExpression, ref stringBuilder);
    stringBuilder.AppendToLastLine($" {Keyword(Keywords.THEN)}");
    ScalarExpression(searchedWhenClause.ThenExpression, ref then);
    stringBuilder.AddNewLines(IndentLines(then));
  }

  /// <summary>
  /// Formats a SELECT clause.
  /// </summary>
  /// <param name="selectClause">
  /// The select clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted select clause to.
  /// </param>
  private void SelectClause(SqlSelectClause selectClause, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine(Keyword(Keywords.SELECT));

    if (selectClause.IsDistinct)
    {
      stringBuilder.AppendToLastLine($" {Keyword(Keywords.DISTINCT)}");
    }

    if (selectClause.Top is SqlTopSpecification topSpecification)
    {
      TopSpecification(topSpecification, ref stringBuilder);
    }

    SelectExpressionCollection(selectClause.SelectExpressions, ref stringBuilder);
  }

  /// <summary>
  /// Formats a select expression.
  /// </summary>
  /// <param name="selectExpression">
  /// The select expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted select expression to.
  /// </param>
  private void SelectExpression(SqlSelectExpression selectExpression, ref StringBuilder stringBuilder)
  {
    switch (selectExpression)
    {
      case SqlSelectScalarExpression selectScalarExpression:
        {
          SelectScalarExpression(selectScalarExpression, ref stringBuilder);

          break;
        }

      case SqlSelectStarExpression selectStarExpression:
        {
          SelectStarExpression(selectStarExpression, ref stringBuilder);

          break;
        }

      case SqlSelectVariableAssignmentExpression selectVariableAssignmentExpression:
        {
          SelectVariableAssignmentExpression(selectVariableAssignmentExpression, ref stringBuilder);

          break;
        }

      default:
        {
          Utils.HandleNotImplemented(selectExpression, ref stringBuilder);

          break;
        }
    }
  }

  /// <summary>
  /// Formats a select expression collection.
  /// </summary>
  /// <param name="selectExpressionCollection">
  /// The select expression collection to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted select expression collection to.
  /// </param>
  private void SelectExpressionCollection(SqlSelectExpressionCollection selectExpressionCollection, ref StringBuilder stringBuilder)
  {
    for (int index = 0; index < selectExpressionCollection.Count; index += 1)
    {
      SqlSelectExpression selectExpression = selectExpressionCollection[index];

      SelectExpression(selectExpression, ref stringBuilder);

      if (index < selectExpressionCollection.Count - 1)
      {
        stringBuilder.AppendToLastLine(",");
      }
    }
  }

  /// <summary>
  /// Formats a SELECT...INTO clause.
  /// </summary>
  /// <param name="selectIntoClause">
  /// The SELECT...INTO clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted SELECT...INTO clause to.
  /// </param>
  private void SelectIntoClause(SqlSelectIntoClause selectIntoClause, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine($"{Keyword(Keywords.INTO)} ");
    ObjectIdentifier(selectIntoClause.IntoTarget, ref stringBuilder);
  }

  /// <summary>
  /// Formats a select scalar expression.
  /// </summary>
  /// <param name="selectScalarExpression">
  /// The select scalar expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted select scalar expression to.
  /// </param>
  private void SelectScalarExpression(SqlSelectScalarExpression selectScalarExpression, ref StringBuilder stringBuilder)
  {
    StringBuilder expression = new();

    ScalarExpression(selectScalarExpression.Expression, ref expression);
    stringBuilder.AddNewLines(IndentLines(expression));

    if (selectScalarExpression.Alias is SqlIdentifier aliasIdentifier)
    {
      stringBuilder.AppendToLastLine($" {Keyword(Keywords.AS)} {Identifier(aliasIdentifier)}");
    }
  }

  /// <summary>
  /// Formats a select specification.
  /// </summary>
  /// <param name="selectSpecification">
  /// The select specification to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted select specification to.
  /// </param>
  private void SelectSpecification(SqlSelectSpecification selectSpecification, ref StringBuilder stringBuilder)
  {
    stringBuilder.AddNewLine();
    QueryExpression(selectSpecification.QueryExpression, ref stringBuilder);

    if (selectSpecification.OrderByClause is SqlOrderByClause orderByClause)
    {
      OrderByClause(orderByClause, ref stringBuilder);
    }
  }

  /// <summary>
  /// Formats a * expression.
  /// </summary>
  /// <param name="selectStarExpression">
  /// The select star expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted select star expression to.
  /// </param>
  private void SelectStarExpression(SqlSelectStarExpression selectStarExpression, ref StringBuilder stringBuilder)
  {
    stringBuilder.AddNewLine(Indentation());

    if (selectStarExpression.Qualifier is SqlObjectIdentifier objectIdentifier)
    {
      ObjectIdentifier(objectIdentifier, ref stringBuilder);
      stringBuilder.AppendToLastLine(".");
    }

    stringBuilder.AppendToLastLine("*");
  }

  /// <summary>
  /// Formats a SELECT statement.
  /// </summary>
  /// <param name="selectStatement">
  /// The SELECT statement to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted SELECT statement to.
  /// </param>
  private void SelectStatement(SqlSelectStatement selectStatement, ref StringBuilder stringBuilder)
  {
    if (selectStatement.QueryWithClause is SqlQueryWithClause queryWithClause)
    {
      QueryWithClause(queryWithClause, ref stringBuilder);
    }

    if (selectStatement.SelectSpecification is SqlSelectSpecification selectSpecification)
    {
      SelectSpecification(selectSpecification, ref stringBuilder);
    }
  }

  /// <summary>
  /// Formats a SELECT variable assignment expression.
  /// </summary>
  /// <param name="selectVariableAssignmentExpression">
  /// The SELECT variable assignment expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted SELECT variable assignment expression to.
  /// </param>
  private void SelectVariableAssignmentExpression(SqlSelectVariableAssignmentExpression selectVariableAssignmentExpression, ref StringBuilder stringBuilder)
  {
    StringBuilder variable = new();

    variable.AppendToLastLine(selectVariableAssignmentExpression.VariableAssignment.Variable.VariableName);
    ComparisonBooleanExpressionType(SqlComparisonBooleanExpressionType.Equals, ref variable);
    ScalarExpression(selectVariableAssignmentExpression.VariableAssignment.Value, ref variable);
    stringBuilder.AddNewLines(IndentLines(variable));
  }

  /// <summary>
  /// Formats a simple CASE expression.
  /// </summary>
  /// <param name="simpleCaseExpression">
  /// The simple CASE expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted simple CASE expression to.
  /// </param>
  private void SimpleCaseExpression(SqlSimpleCaseExpression simpleCaseExpression, ref StringBuilder stringBuilder)
  {
    stringBuilder.AppendToLastLine($"{Keyword(Keywords.CASE)} ");
    ScalarExpression(simpleCaseExpression.TestExpression, ref stringBuilder);

    foreach (SqlSimpleWhenClause simpleWhenClause in simpleCaseExpression.WhenClauses)
    {
      SimpleWhenClause(simpleWhenClause, ref stringBuilder);
    }

    if (simpleCaseExpression.ElseExpression is SqlScalarExpression elseExpression)
    {
      StringBuilder _else = new();

      stringBuilder.AddNewLine(Keyword(Keywords.ELSE));
      ScalarExpression(elseExpression, ref _else);
      stringBuilder.AddNewLines(IndentLines(_else));
    }

    stringBuilder.AddNewLine(Keyword(Keywords.END));
  }

  /// <summary>
  /// Formats a simple GROUP BY item.
  /// </summary>
  /// <param name="simpleGroupByItem">
  /// The simple GROUP BY item to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted simple GROUP BY item to.
  /// </param>
  private void SimpleGroupByItem(SqlSimpleGroupByItem simpleGroupByItem, ref StringBuilder stringBuilder)
  {
    ScalarExpression(simpleGroupByItem.Expression, ref stringBuilder);
  }

  /// <summary>
  /// Formats a simple WHEN clause.
  /// </summary>
  /// <param name="simpleWhenClause">
  /// The simple WHEN clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted simple WHEN clause to.
  /// </param>
  private void SimpleWhenClause(SqlSimpleWhenClause simpleWhenClause, ref StringBuilder stringBuilder)
  {
    StringBuilder then = new();

    stringBuilder.AddNewLine($"{Keyword(Keywords.WHEN)} ");
    ScalarExpression(simpleWhenClause.WhenExpression, ref stringBuilder);
    stringBuilder.AppendToLastLine($" {Keyword(Keywords.THEN)}");
    ScalarExpression(simpleWhenClause.ThenExpression, ref then);
    stringBuilder.AddNewLines(IndentLines(then));
  }

  /// <summary>
  /// Formats a sort order.
  /// </summary>
  /// <param name="sortOrder">
  /// The sort order to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted sort order to.
  /// </param>
  private void SortOrder(SqlSortOrder sortOrder, ref StringBuilder stringBuilder)
  {
    string raw = sortOrder switch
    {
      SqlSortOrder.Ascending => Keyword(Keywords.ASC),
      SqlSortOrder.Descending => Keyword(Keywords.DESC),
      SqlSortOrder.None => string.Empty,
      _ => sortOrder.ToString(),
    };
    string value = string.IsNullOrWhiteSpace(raw) ? string.Empty : $" {raw}";

    stringBuilder.AppendToLastLine(value);
  }

  /// <summary>
  /// Formats a generic SQL statement.
  /// </summary>
  /// <param name="statement">
  /// The SQL statement to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted SQL statement to.
  /// </param>
  private void Statement(SqlStatement statement, ref StringBuilder stringBuilder)
  {
    switch (statement)
    {
      case SqlDmlStatement dmlStatement:
        {
          DmlStatement(dmlStatement, ref stringBuilder);

          break;
        }

      case SqlSelectStatement selectStatement:
        {
          SelectStatement(selectStatement, ref stringBuilder);

          break;
        }

      // TODO

      default:
        {
          Utils.HandleNotImplemented(statement, ref stringBuilder);

          break;
        }
    }

    stringBuilder.AppendToLastLine(";");

    // Add configured number of blank lines between statements.
    foreach (int _ in Enumerable.Range(0, Options.LinesBetweenStatements))
    {
      stringBuilder.AddNewLine();
    }
  }

  /// <summary>
  /// Formats a table constructor expression.
  /// </summary>
  /// <param name="tableConstructorExpression">
  /// The table constructor expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted table constructor expression to.
  /// </param>
  private void TableConstructorExpression(SqlTableConstructorExpression tableConstructorExpression, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(tableConstructorExpression, ref stringBuilder);
  }

  /// <summary>
  /// Formats a table expression.
  /// </summary>
  /// <param name="tableExpression">
  /// The table expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted table expression to.
  /// </param>
  private void TableExpression(SqlTableExpression tableExpression, ref StringBuilder stringBuilder)
  {
    switch (tableExpression)
    {
      case SqlJoinTableExpression joinTableExpression:
        {
          JoinTableExpression(joinTableExpression, ref stringBuilder);

          break;
        }

      case SqlTableRefExpression tableRefExpression:
        {
          TableRefExpression(tableRefExpression, ref stringBuilder);

          break;
        }

      // TODO

      default:
        {
          Utils.HandleNotImplemented(tableExpression, ref stringBuilder);

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
  /// <param name="stringBuilder">
  /// The string builder to append the formatted table hint to.
  /// </param>
  private void TableHint(SqlTableHint tableHint, ref StringBuilder stringBuilder)
  {
    stringBuilder.AddNewLine();
    TableHintType(tableHint.Type, ref stringBuilder);
  }

  /// <summary>
  /// Formats a table hint type.
  /// </summary>
  /// <param name="tableHintType">
  /// The table hint type to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted table hint type to.
  /// </param>
  private void TableHintType(SqlTableHintType tableHintType, ref StringBuilder stringBuilder)
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

    stringBuilder.AppendToLastLine(value);
  }

  /// <summary>
  /// Formats a table reference expression.
  /// </summary>
  /// <param name="tableRefExpression">
  /// The table reference expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted table reference expression to.
  /// </param>
  private void TableRefExpression(SqlTableRefExpression tableRefExpression, ref StringBuilder stringBuilder)
  {
    ObjectIdentifier(tableRefExpression.ObjectIdentifier, ref stringBuilder);

    if (tableRefExpression.Alias is SqlIdentifier aliasIdentifier)
    {
      stringBuilder.AddNewLine($"{Indentation()}{Keyword(Keywords.AS)} {Identifier(aliasIdentifier)}");
    }

    if (tableRefExpression.Hints is SqlHintCollection hintCollection)
    {
      StringBuilder _hint = new();

      foreach (SqlHint hint in hintCollection)
      {
        Hint(hint, ref _hint);
      }

      if (_hint.IsMultiLine)
      {
        stringBuilder
          .AddNewLine($"{Indentation()}{Keyword(Keywords.WITH)} (")
          .AddNewLines(IndentLines(_hint))
          .AddNewLine($"{Indentation()})");
      }
      else
      {
        stringBuilder.AddNewLine($"{Indentation()}{Keyword(Keywords.WITH)} ({_hint})");
      }
    }
  }

  /// <summary>
  /// Formats a TOP specification.
  /// </summary>
  /// <param name="topSpecification">
  /// The TOP specification to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted TOP specification to.
  /// </param>
  private void TopSpecification(SqlTopSpecification topSpecification, ref StringBuilder stringBuilder)
  {
    StringBuilder topValue = new();

    stringBuilder.AppendToLastLine($" {Keyword(Keywords.TOP)} ");
    ScalarExpression(topSpecification.Value, ref topValue);

    if (topValue.IsMultiLine)
    {
      stringBuilder
        .AppendToLastLine("(")
        .AddNewLines(IndentLines(topValue))
        .AddNewLine(")");
    }
    else
    {
      stringBuilder.AppendToLastLine(topValue);
    }

    if (topSpecification.IsPercent)
    {
      stringBuilder.AppendToLastLine($" {Keyword(Keywords.PERCENT)}");
    }

    if (topSpecification.IsWithTies)
    {
      stringBuilder.AppendToLastLine($" {Keyword(Keywords.WITH_TIES)}");
    }
  }

  /// <summary>
  /// Formats a UDT member expression.
  /// </summary>
  /// <param name="udtMemberExpression">
  /// The UDT member expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted UDT member expression to.
  /// </param>
  private void UdtMemberExpression(SqlUdtMemberExpression udtMemberExpression, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(udtMemberExpression, ref stringBuilder);
  }

  /// <summary>
  /// Formats a unary scalar expression.
  /// </summary>
  /// <param name="unaryScalarExpression">
  /// The unary scalar expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted unary scalar expression to.
  /// </param>
  private void UnaryScalarExpression(SqlUnaryScalarExpression unaryScalarExpression, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(unaryScalarExpression, ref stringBuilder);
  }

  /// <summary>
  /// Formats an UPDATE boolean expression.
  /// </summary>
  /// <param name="updateBooleanExpression">
  /// The UPDATE boolean expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted UPDATE boolean expression to.
  /// </param>
  private void UpdateBooleanExpression(SqlUpdateBooleanExpression updateBooleanExpression, ref StringBuilder stringBuilder)
  {
    Utils.HandleNotImplemented(updateBooleanExpression, ref stringBuilder);
  }

  /// <summary>
  /// Formats a user-defined scalar function call expression.
  /// </summary>
  /// <param name="userDefinedScalarFunctionCallExpression">
  /// The user-defined scalar function call expression to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted user-defined scalar function call expression to.
  /// </param>
  private void UserDefinedScalarFunctionCallExpression(SqlUserDefinedScalarFunctionCallExpression userDefinedScalarFunctionCallExpression, ref StringBuilder stringBuilder)
  {
    ObjectIdentifier(userDefinedScalarFunctionCallExpression.ObjectIdentifier, ref stringBuilder);
    stringBuilder.AppendToLastLine("(");

    if (userDefinedScalarFunctionCallExpression.Arguments is SqlScalarExpressionCollection scalarExpressionCollection)
    {
      ScalarExpressionCollection(scalarExpressionCollection, ref stringBuilder);
    }

    stringBuilder.AppendToLastLine(")");
  }

  /// <summary>
  /// Formats a WHERE clause.
  /// </summary>
  /// <param name="whereClause">
  /// The WHERE clause to format.
  /// </param>
  /// <param name="stringBuilder">
  /// The string builder to append the formatted WHERE clause to.
  /// </param>
  private void WhereClause(SqlWhereClause whereClause, ref StringBuilder stringBuilder)
  {
    StringBuilder booleanExpression = new();

    stringBuilder.AppendToLastLine(Keyword(Keywords.WHERE));
    BooleanExpression(whereClause.Expression, ref booleanExpression);
    stringBuilder.AddNewLines(IndentLines(booleanExpression));
  }
}
