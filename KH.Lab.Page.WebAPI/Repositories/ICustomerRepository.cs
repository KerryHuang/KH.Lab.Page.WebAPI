using KH.Lab.Page.WebAPI.Models;


namespace KH.Lab.Page.WebAPI.Repositories;
public interface ICustomerRepository
{
    Task<PagedResults<Customer>> GetCustomersAsync(string searchString = "", int pageNumber = 1, int pageSize = 10);
}