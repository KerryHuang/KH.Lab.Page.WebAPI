# .NET 8 Web API 使用 Dapper 做分頁查詢

用 Dapper 對 SQL Server 的 Northwind 資料庫做分頁查詢，示範一次往返就拿到「這一頁的資料」和「總筆數」，再算出前端分頁列需要的頁碼。

## 技術

- .NET 8、ASP.NET Core Web API
- Dapper 2.1、Microsoft.Data.SqlClient
- Swagger（Swashbuckle）
- SQL Server LocalDB＋Northwind 範例資料庫

## 怎麼跑

1. 準備 Northwind 資料庫：在 LocalDB（`(localdb)\mssqllocaldb`）建立 `Northwind`。微軟官方的建立腳本在 [microsoft/sql-server-samples](https://github.com/microsoft/sql-server-samples/tree/master/samples/databases/northwind-pubs) 的 `instnwnd.sql`。
2. 連線字串在 `KH.Lab.Page.WebAPI/appsettings.json` 的 `ConnectionStrings:DefaultConnection`，資料庫不在 LocalDB 時改這裡。
3. 執行：

   ```bash
   dotnet run --project KH.Lab.Page.WebAPI
   ```

4. 瀏覽器會開 Swagger（`http://localhost:5062/swagger`），可以直接試 API。

## API

| 方法 | 路徑 | 說明 |
|---|---|---|
| GET | `/api/Customer` | 第 1 頁，每頁 10 筆 |
| GET | `/api/Customer/{page}` | 指定頁碼，每頁 10 筆 |

回傳的 `PagedResults<Customer>`：

```json
{
  "items": [ { "customerID": "ALFKI", "companyName": "Alfreds Futterkiste", "contactName": "Maria Anders", "contactTitle": "Sales Representative" } ],
  "totalItems": 91,
  "pageSize": 10,
  "pageNumber": 1,
  "maxNavigationPages": 5,
  "totalPages": 10,
  "startPage": 1,
  "endPage": 5,
  "pageNumbers": [1, 2, 3, 4, 5]
}
```

## 重點程式

### 一次往返拿資料與總筆數（`Repositories/CustomerRepository.cs`）

兩段 SQL 用分號串成一個批次，再用 Dapper 的 `QueryMultipleAsync` 依序讀出兩個結果集：

```csharp
var queries = "SELECT * FROM [dbo].[Customers] (NOLOCK) "
            + "ORDER BY [CustomerID] OFFSET @PageSize * (@PageNumber - 1) ROWS FETCH NEXT @PageSize ROWS ONLY;"
            + "SELECT COUNT(*) AS TotalItems FROM [dbo].[Customers] (NOLOCK);";

var multi = await connection.QueryMultipleAsync(queries, new { PageNumber = pageNumber, PageSize = pageSize });
var items = (await multi.ReadAsync<Customer>()).ToList();
var totalItems = await multi.ReadFirstAsync<int>();
```

- 分頁用 SQL Server 2012 起支援的 `OFFSET … FETCH NEXT`。
- 程式裡另外留了一段註解掉的 `ROW_NUMBER() OVER (...)` 寫法，適用更舊的 SQL Server，可以對照兩種做法。
- 頁碼和每頁筆數都走參數（`@PageNumber`、`@PageSize`），不是字串拼接，沒有 SQL injection 的問題。

### 分頁列的頁碼計算（`Models/PagedResults.cs`）

`PagedResults<T>` 依總筆數、目前頁碼、每頁筆數，算出前端分頁列要顯示哪幾頁：

- `TotalPages`：總筆數除以每頁筆數，無條件進位。
- `PageNumber`：把傳進來的頁碼夾在 1 到總頁數之間，超出範圍不會出錯。
- `StartPage`、`EndPage`、`PageNumbers`：以目前頁碼為中心，最多顯示 `MaxNavigationPages`（預設 5）個頁碼；靠近頭尾時整段往內縮，頁碼數維持一樣多。

例如總共 10 頁、目前在第 6 頁，分頁列顯示 4、5、6、7、8；在第 10 頁時顯示 6 到 10。

## 已知限制

- `GetCustomersAsync` 有 `searchString` 參數，但搜尋條件那段程式是註解掉的，目前沒有作用。
- 每頁筆數固定是 10，API 沒有開放調整。
- `(NOLOCK)` 會讀到尚未 commit 的資料。範例用來簡化示範，正式環境要依需求評估。
- 專案裡的 `WeatherForecastController` 是 Visual Studio 範本自帶的，跟分頁無關。

## 授權

[MIT](LICENSE.txt)
