using tsql_formatter;

TransactSQLFormatterOptions options = new()
{
  IndentationString = "  ",
  IdentifierStyle = TransactSQLFormatterOptions.IdentifierStyles.SquareBrackets,
  KeywordCase = TransactSQLFormatterOptions.KeywordCases.Uppercase,
  LinesBetweenStatements = 1,
  OperatorSpacing = TransactSQLFormatterOptions.OperatorSpacings.SpaceAround,
};
TransactSQLFormatter formatter = new(options);
string sql = @"
  SELECT ProductName FROM Products WHERE Price > ALL (SELECT Price FROM Products WHERE Category = 'Electronics');

  with cte (dada) as (
    select distinct top 2 e.e as b
    from dbo.[Employees] as e with (nolock)
    where [Status] = 'Active'
  )
  select [C].bob as u, *, @var = (select 1)
  into dbo.[ActiveUsers]
  from cte /* a comment inside a select */ as [C]
  where [C].[Active] = 1 AND [C].[Role] > 1
  GROUP BY [foo]
  having count(*) > 5
  order by a asc
  OFFSET null ROWS FETCH NEXT 5 ROWS ONLY;
  -- some inline comment

  select top(100) I.*
  from [Sales].[Invoices] as I
  where Role = N'Admin'
  order by foo
  offset (
    select 0
  ) rows;

  delete from [dbo].[Users]
  where UserId = 10;

  /*
  some block comment
  */

  update dbo.[Orders]
  set Status = 'Shipped'
  where [OrderId] = 1001;

  insert into dbo.[Products]
  (Name, [Price])
  values
  ('Gadget', 19.99);

  if exists (select 1 from dbo.[Logs] where [Level] = 'Error')
  begin
    raiserror('Errors found in logs', 16, 1);
  end

  if object_id('tempdb..#TempTable') is not null
    drop table #TempTable;
";

Console.WriteLine(formatter.FormatTsql(sql));
