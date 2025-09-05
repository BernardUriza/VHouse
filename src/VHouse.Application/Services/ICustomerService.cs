using VHouse.Domain.Entities;
using VHouse.Application.DTOs;

namespace VHouse.Application.Services;

public interface ICustomerService
{
    Task<IEnumerable<Customer>> GetActiveCustomersAsync(int? clientTenantId = null);
    Task<Customer?> GetCustomerByIdAsync(int customerId);
    Task<Customer> CreateCustomerAsync(CreateCustomerDto dto, int? clientTenantId = null);
    Task<Customer> UpdateCustomerAsync(int customerId, UpdateCustomerDto dto);
    Task<bool> DeactivateCustomerAsync(int customerId);
    Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm, int? clientTenantId = null);
    Task<CustomerOrderSummaryDto> GetCustomerOrderSummaryAsync(int customerId);
}