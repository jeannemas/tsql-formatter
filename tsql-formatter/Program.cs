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
string insert = @"
  INSERT
  INTO Production.UnitMeasure
  VALUES (N'FT', N'Feet', '20080414');

  INSERT
  INTO Production.UnitMeasure
  VALUES (N'FT2', N'Square Feet ', '20080923'), (N'Y', N'Yards', '20080923'), (N'Y3', N'Cubic Yards', '20080923');

  INSERT
  INTO Production.UnitMeasure (Name, UnitMeasureCode, ModifiedDate)
  VALUES (N'Square Yards', N'Y2', GETDATE());

  INSERT
  INTO dbo.T1 (column_4)
  VALUES ('Explicit value');

  INSERT
  INTO dbo.T1 (column_2, column_4)
  VALUES ('Explicit value', 'Explicit value');

  INSERT
  INTO dbo.T1 (column_2)
  VALUES ('Explicit value');

  INSERT
  INTO T1
  DEFAULT VALUES;

  INSERT T1
  VALUES ('Row #1');

  INSERT T1 (column_2)
  VALUES ('Row #2');

  INSERT
  INTO T1 (column_1,column_2)
  VALUES (-99, 'Explicit identity value');

  INSERT
  INTO dbo.T1 (column_2)
  VALUES (NEWID());

  INSERT
  INTO T1
  DEFAULT VALUES;

  INSERT
  INTO dbo.Points (PointValue)
  VALUES (CONVERT(Point, '3,4'));

  INSERT
  INTO dbo.Points (PointValue)
  VALUES (CONVERT(Point, '1,5'));

  INSERT
  INTO dbo.Points (PointValue)
  VALUES (CAST ('1,99' AS Point));

  INSERT
  INTO dbo.EmployeeSales
  SELECT 'SELECT', sp.BusinessEntityID, c.LastName, sp.SalesYTD
  FROM Sales.SalesPerson AS sp
  INNER JOIN Person.Person AS c ON sp.BusinessEntityID = c.BusinessEntityID
  WHERE sp.BusinessEntityID LIKE '2%'
  ORDER BY sp.BusinessEntityID, c.LastName;

  INSERT INTO dbo.EmployeeSales
  EXECUTE dbo.uspGetEmployeeSales;

  INSERT INTO dbo.EmployeeSales
  EXECUTE ('
    SELECT ''EXEC STRING'', sp.BusinessEntityID, c.LastName, sp.SalesYTD
    FROM Sales.SalesPerson AS sp
    INNER JOIN Person.Person AS c ON sp.BusinessEntityID = c.BusinessEntityID
    WHERE sp.BusinessEntityID LIKE ''2%''
    ORDER BY sp.BusinessEntityID, c.LastName
  ');

  WITH EmployeeTemp (EmpID, LastName, FirstName, Phone, Address, City, StateProvince, PostalCode, CurrentFlag)
  AS (
    SELECT e.BusinessEntityID, c.LastName, c.FirstName, pp.PhoneNumber, a.AddressLine1, a.City, sp.StateProvinceCode, a.PostalCode, e.CurrentFlag
    FROM HumanResources.Employee e
    INNER JOIN Person.BusinessEntityAddress AS bea ON e.BusinessEntityID = bea.BusinessEntityID
    INNER JOIN Person.Address AS a ON bea.AddressID = a.AddressID
    INNER JOIN Person.PersonPhone AS pp ON e.BusinessEntityID = pp.BusinessEntityID
    INNER JOIN Person.StateProvince AS sp ON a.StateProvinceID = sp.StateProvinceID
    INNER JOIN Person.Person as c ON e.BusinessEntityID = c.BusinessEntityID
  )
  INSERT
  INTO HumanResources.NewEmployee
  SELECT EmpID, LastName, FirstName, Phone, Address, City, StateProvince, PostalCode, CurrentFlag
  FROM EmployeeTemp;

  INSERT TOP (5)
  INTO dbo.EmployeeSales
  OUTPUT inserted.EmployeeID, inserted.FirstName, inserted.LastName, inserted.YearlySales
  SELECT sp.BusinessEntityID, c.LastName, c.FirstName, sp.SalesYTD
  FROM Sales.SalesPerson AS sp
  INNER JOIN Person.Person AS c ON sp.BusinessEntityID = c.BusinessEntityID
  WHERE sp.SalesYTD > 250000.00
  ORDER BY sp.SalesYTD DESC;

  INSERT
  INTO dbo.EmployeeSales
  OUTPUT inserted.EmployeeID, inserted.FirstName, inserted.LastName, inserted.YearlySales
  SELECT TOP (5) sp.BusinessEntityID, c.LastName, c.FirstName, sp.SalesYTD
  FROM Sales.SalesPerson AS sp
  INNER JOIN Person.Person AS c ON sp.BusinessEntityID = c.BusinessEntityID
  WHERE sp.SalesYTD > 250000.00
  ORDER BY sp.SalesYTD DESC;

  INSERT
  INTO V1
  VALUES ('Row 1',1);

  INSERT
  INTO @MyTableVar (LocationID, CostRate, ModifiedDate)
  SELECT LocationID, CostRate, GETDATE()
  FROM Production.Location
  WHERE CostRate > 0;

  INSERT
  INTO Sales.SalesHistory WITH (TABLOCK) (SalesOrderID, SalesOrderDetailID, CarrierTrackingNumber, OrderQty, ProductID, SpecialOfferID, UnitPrice, UnitPriceDiscount, LineTotal, rowguid, ModifiedDate)
  SELECT *
  FROM Sales.SalesOrderDetail;

  INSERT
  INTO Production.Location WITH (XLOCK) (Name, CostRate, Availability)
  VALUES ( N'Final Inventory', 15.00, 80.00);

  INSERT Production.ScrapReason
  OUTPUT INSERTED.ScrapReasonID, INSERTED.Name, INSERTED.ModifiedDate INTO @MyTableVar
  VALUES (N'Operator error', GETDATE());

  INSERT 
  INTO dbo.EmployeeSales (LastName, FirstName, CurrentSales)
  OUTPUT INSERTED.LastName, INSERTED.FirstName, INSERTED.CurrentSales INTO @MyTableVar
  SELECT c.LastName, c.FirstName, sp.SalesYTD
  FROM Sales.SalesPerson AS sp
  INNER JOIN Person.Person AS c ON sp.BusinessEntityID = c.BusinessEntityID
  WHERE sp.BusinessEntityID LIKE '2%'
  ORDER BY c.LastName, c.FirstName;

  INSERT
  INTO Production.ZeroInventory (DeletedProductID, RemovedOnDate)
  SELECT ProductID, GETDATE()
  FROM (
    MERGE Production.ProductInventory AS pi
    USING (
      SELECT ProductID, SUM(OrderQty)
      FROM Sales.SalesOrderDetail AS sod
      JOIN Sales.SalesOrderHeader AS soh ON sod.SalesOrderID = soh.SalesOrderID AND soh.OrderDate = '20070401'
      GROUP BY ProductID
    ) AS src (ProductID, OrderQty) ON (pi.ProductID = src.ProductID)
    WHEN MATCHED AND pi.Quantity - src.OrderQty <= 0 THEN DELETE
    WHEN MATCHED THEN UPDATE SET pi.Quantity = pi.Quantity - src.OrderQty
    OUTPUT $action, deleted.ProductID
  ) AS Changes (Action, ProductID)
  WHERE Action = 'DELETE';

  INSERT
  INTO EmployeeTitles
  SELECT EmployeeKey, LastName, Title
  FROM ssawPDW.dbo.DimEmployee
  WHERE EndDate IS NULL;

  INSERT
  INTO DimCurrency
  VALUES (500, N'C1', N'Currency1')
  OPTION ( LABEL = N'label1' );

  INSERT
  INTO DimCustomer (CustomerKey, CustomerAlternateKey, FirstName, MiddleName, LastName )
  SELECT ProspectiveBuyerKey, ProspectAlternateKey, FirstName, MiddleName, LastName
  FROM ProspectiveBuyer p
  JOIN DimGeography g ON p.PostalCode = g.PostalCode
  WHERE g.CountryRegionCode = 'FR'
  OPTION ( LABEL = 'Add French Prospects', HASH JOIN);
";
string select = @"
  SELECT *
  FROM DimEmployee
  ORDER BY LastName;

  SELECT e.*
  FROM DimEmployee AS e
  ORDER BY LastName;

  SELECT FirstName, LastName, StartDate AS FirstDay
  FROM DimEmployee
  ORDER BY LastName;

  SELECT FirstName, LastName, StartDate AS FirstDay
  FROM DimEmployee
  WHERE EndDate IS NOT NULL AND MaritalStatus = 'M'
  ORDER BY LastName;

  SELECT FirstName, LastName, BaseRate, BaseRate * 40 AS GrossPay
  FROM DimEmployee
  ORDER BY LastName;

  SELECT DISTINCT Title
  FROM DimEmployee
  ORDER BY Title;

  SELECT OrderDateKey, SUM(SalesAmount) AS TotalSales
  FROM FactInternetSales
  GROUP BY OrderDateKey
  ORDER BY OrderDateKey;

  SELECT OrderDateKey, PromotionKey, AVG(SalesAmount) AS AvgSales, SUM(SalesAmount) AS TotalSales
  FROM FactInternetSales
  GROUP BY OrderDateKey, PromotionKey
  ORDER BY OrderDateKey;

  SELECT OrderDateKey, SUM(SalesAmount) AS TotalSales
  FROM FactInternetSales
  WHERE OrderDateKey > '20020801'
  GROUP BY OrderDateKey
  ORDER BY OrderDateKey;

  SELECT SUM(SalesAmount) AS TotalSales
  FROM FactInternetSales
  GROUP BY (OrderDateKey * 10);

  SELECT OrderDateKey, SUM(SalesAmount) AS TotalSales
  FROM FactInternetSales
  GROUP BY OrderDateKey
  ORDER BY OrderDateKey;

  SELECT OrderDateKey, SUM(SalesAmount) AS TotalSales
  FROM FactInternetSales
  GROUP BY OrderDateKey
  HAVING OrderDateKey > 20010000
  ORDER BY OrderDateKey;

  SELECT SalesOrderID, SUM(LineTotal) AS SubTotal
  FROM Sales.SalesOrderDetail AS sod
  GROUP BY SalesOrderID
  ORDER BY SalesOrderID;

  SELECT a.City, COUNT(bea.AddressID) EmployeeCount
  FROM Person.BusinessEntityAddress AS bea
  INNER JOIN Person.Address AS a ON bea.AddressID = a.AddressID
  GROUP BY a.City
  ORDER BY a.City;

  SELECT DATEPART(yyyy,OrderDate) AS N'Year',SUM(TotalDue) AS N'Total Order Amount'
  FROM Sales.SalesOrderHeader
  GROUP BY DATEPART(yyyy,OrderDate)
  ORDER BY DATEPART(yyyy,OrderDate);

  SELECT DATEPART(yyyy,OrderDate) AS N'Year',SUM(TotalDue) AS N'Total Order Amount'
  FROM Sales.SalesOrderHeader
  GROUP BY DATEPART(yyyy,OrderDate)
  HAVING DATEPART(yyyy,OrderDate) >= N'2003'
  ORDER BY DATEPART(yyyy,OrderDate);

  SELECT SalesOrderID, SUM(LineTotal) AS SubTotal
  FROM Sales.SalesOrderDetail
  GROUP BY SalesOrderID
  HAVING SUM(LineTotal) > 100000.00
  ORDER BY SalesOrderID;

  SELECT c.FirstName, c.LastName, e.JobTitle, a.AddressLine1, a.City, sp.Name AS [State/Province], a.PostalCode
  INTO dbo.EmployeeAddresses
  FROM Person.Person AS c
  JOIN HumanResources.Employee AS e ON e.BusinessEntityID = c.BusinessEntityID
  JOIN Person.BusinessEntityAddress AS bea ON e.BusinessEntityID = bea.BusinessEntityID
  JOIN Person.Address AS a ON bea.AddressID = a.AddressID
  JOIN Person.StateProvince as sp ON sp.StateProvinceID = a.StateProvinceID;

  SELECT * INTO dbo.NewProducts
  FROM Production.Product
  WHERE ListPrice > $25 AND ListPrice < $100;

  SELECT OBJECT_NAME(object_id) AS TableName, name AS column_name, is_identity, seed_value, increment_value
  FROM sys.identity_columns
  WHERE name = 'AddressID';

  SELECT IDENTITY (int, 100, 5) AS AddressID, a.AddressLine1, a.City, b.Name AS State, a.PostalCode
  INTO Person.USAddress
  FROM Person.Address AS a
  INNER JOIN Person.StateProvince AS b ON a.StateProvinceID = b.StateProvinceID
  WHERE b.CountryRegionCode = N'US';

  SELECT OBJECT_NAME(object_id) AS TableName, name AS column_name, is_identity, seed_value, increment_value
  FROM sys.identity_columns
  WHERE name = 'AddressID';

  SELECT ProductID, Name
  FROM Production.Product
  WHERE Name LIKE 'Lock Washer%'
  ORDER BY ProductID;

  SELECT ProductID, Name, Color
  FROM Production.Product
  ORDER BY ListPrice;

  SELECT name, SCHEMA_NAME(schema_id) AS SchemaName
  FROM sys.objects
  WHERE type = 'U'
  ORDER BY SchemaName;

  SELECT BusinessEntityID, JobTitle, HireDate
  FROM HumanResources.Employee
  ORDER BY DATEPART(year, HireDate);

  SELECT ProductID, Name
  FROM Production.Product
  WHERE Name LIKE 'Lock Washer%'
  ORDER BY ProductID DESC;

  SELECT ProductID, Name
  FROM Production.Product
  WHERE Name LIKE 'Lock Washer%'
  ORDER BY Name ASC;

  SELECT LastName, FirstName
  FROM Person.Person
  WHERE LastName LIKE 'R%'
  ORDER BY FirstName ASC, LastName DESC;

  SELECT name
  FROM #t1
  ORDER BY name;

  SELECT name
  FROM #t1
  ORDER BY name COLLATE Latin1_General_CS_AS;

  SELECT BusinessEntityID, SalariedFlag
  FROM HumanResources.Employee
  ORDER BY CASE SalariedFlag WHEN 1 THEN BusinessEntityID END DESC, CASE WHEN SalariedFlag = 0 THEN BusinessEntityID END;

  SELECT BusinessEntityID, LastName, TerritoryName, CountryRegionName
  FROM Sales.vSalesPerson
  WHERE TerritoryName IS NOT NULL
  ORDER BY CASE CountryRegionName WHEN 'United States' THEN TerritoryName ELSE CountryRegionName END;

  SELECT p.FirstName, p.LastName, ROW_NUMBER() OVER (ORDER BY a.PostalCode) AS ""Row Number"", RANK() OVER (ORDER BY a.PostalCode) AS ""Rank"", DENSE_RANK() OVER (ORDER BY a.PostalCode) AS ""Dense Rank"", NTILE(4) OVER (ORDER BY a.PostalCode) AS ""Quartile"", s.SalesYTD, a.PostalCode
  FROM Sales.SalesPerson AS s
  INNER JOIN Person.Person AS p ON s.BusinessEntityID = p.BusinessEntityID
  INNER JOIN Person.Address AS a ON a.AddressID = p.BusinessEntityID
  WHERE TerritoryID IS NOT NULL AND SalesYTD <> 0;

  SELECT DepartmentID, Name, GroupName
  FROM HumanResources.Department
  ORDER BY DepartmentID;

  SELECT DepartmentID, Name, GroupName
  FROM HumanResources.Department
  ORDER BY DepartmentID
  OFFSET 5 ROWS;

  SELECT DepartmentID, Name, GroupName
  FROM HumanResources.Department
  ORDER BY DepartmentID
  OFFSET 0 ROWS
  FETCH NEXT 10 ROWS ONLY;

  SELECT DepartmentID, Name, GroupName
  FROM HumanResources.Department
  ORDER BY DepartmentID ASC
  OFFSET @RowsToSkip ROWS
  FETCH NEXT @FetchRows ROWS ONLY;

  SELECT DepartmentID, Name, GroupName
  FROM HumanResources.Department
  ORDER BY DepartmentID ASC
  OFFSET @StartingRowNumber - 1 ROWS
  FETCH NEXT @EndingRowNumber - @StartingRowNumber + 1 ROWS ONLY
  OPTION (OPTIMIZE FOR (@StartingRowNumber = 1, @EndingRowNumber = 20));

  SELECT DepartmentID, Name, GroupName
  FROM HumanResources.Department
  ORDER BY DepartmentID ASC
  OFFSET @StartingRowNumber ROWS
  FETCH NEXT (
    SELECT PageSize
    FROM dbo.AppSettings
    WHERE AppSettingID = 1
  ) ROWS ONLY;

  SELECT Name, Color, ListPrice
  FROM Production.Product
  WHERE Color = 'Red'
  UNION ALL SELECT Name, Color, ListPrice
  FROM Production.Product
  WHERE Color = 'Yellow'
  ORDER BY ListPrice ASC;

  SELECT @@VERSION AS SQLVersion;

  SELECT Region, Product, SUM(Amount) AS TotalAmount
  FROM Sales
  GROUP BY Region, Product
  WITH ROLLUP;
";
string update = @"";
string delete = @"";
IEnumerable<string> scripts = [
  // insert,
  select,
  // update,
  // delete,
];

foreach (string script in scripts)
{
  Console.WriteLine(formatter.FormatTsql(script));
}
