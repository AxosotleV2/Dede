using Dede.Domain.Entities;

namespace Dede.Domain.Interfaces;

public interface IServiceCatalogService
{
    Task<List<ServiceItem>> GetServicesAsync(string? sort);
    Task<ServiceItem?> GetByIdAsync(int id);
    Task<ServiceItem> CreateAsync(ServiceItem service);
    Task<ServiceItem?> UpdateAsync(ServiceItem service);
    Task<bool> DeleteAsync(int id);
}