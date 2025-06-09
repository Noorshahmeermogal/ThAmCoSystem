using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThAmCo.WebApi.DTOs;

namespace ThAmCo.WebApi.Services
{
    public interface ICustomerService
    {
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateDto);
        Task<CustomerFundsDto?> GetCustomerFundsAsync(int id);
        Task<bool> RequestAccountDeletionAsync(int id);
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(int page = 1, int pageSize = 20);
        Task<CustomerDetailDto?> GetCustomerDetailAsync(int id);
        Task<bool> DeleteCustomerAsync(int id, string deletedBy);
        Task<CustomerDto?> AddFundsAsync(int customerId, decimal amount, string staffEmail);
        Task<CustomerDto?> CustomerAddFundsAsync(int customerId, decimal amount);
    }
} 