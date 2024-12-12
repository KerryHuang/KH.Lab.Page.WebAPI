using Dapper;
using KH.Lab.Page.WebAPI.Models;
using Microsoft.Data.SqlClient;

namespace KH.Lab.Page.WebAPI.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IConfiguration _configuration;

    public CustomerRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<PagedResults<Customer>> GetCustomersAsync(string searchString = "", int pageNumber = 1, int pageSize = 10)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Set first query
        //var whereStatement = string.IsNullOrWhiteSpace(searchString) ? "" : $"WHERE [FirstName] LIKE '{searchString}'";
        //第一種寫法
        //var queries = " SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY CustomerID DESC) AS 'RowNum',* ";
        //queries += " FROM Customers";
        //queries += " )t WHERE t.RowNum BETWEEN		";
        //queries += " ((@PageNumber-1)*@PageSize +1) AND (@PageNumber*@PageSize) ";
        //第二種寫法
        var queries = "SELECT  * FROM [dbo].[Customers] (NOLOCK) ";
        //queries += whereStatement;
        queries += "ORDER BY [CustomerID]  OFFSET @PageSize * (@PageNumber - 1) ROWS FETCH NEXT @PageSize ROWS ONLY;";

        // Set second query, separated with semi-colon
        queries += "SELECT COUNT(*) AS TotalItems FROM [dbo].[Customers] (NOLOCK);";

        var parameters = new
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var multi = await connection.QueryMultipleAsync(queries, parameters);

        var items = (await multi.ReadAsync<Customer>()).ToList();
        var totalItems = await multi.ReadFirstAsync<int>();
        var result = new PagedResults<Customer>(totalItems, pageNumber, pageSize)
        {
            Items = items
        };

        return result;
    }
}
