using KH.Lab.Page.WebAPI.Models;
using KH.Lab.Page.WebAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace KH.Lab.Page.WebAPI.Controllers;

[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerRepository customerRepository, ILogger<CustomerController> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<PagedResults<Customer>> Get()
    {
        _logger.LogInformation("Fetching customers with default parameters.");
        return await _customerRepository.GetCustomersAsync();
    }

    [HttpGet("{page}")]
    public async Task<PagedResults<Customer>> Get(int page)
    {
        _logger.LogInformation("Fetching customers for page {PageNumber}.", page);
        return await _customerRepository.GetCustomersAsync("", page);
    }
}
