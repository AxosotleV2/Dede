using Dede.Domain.Entities;

namespace Dede.Domain.Interfaces;

public interface IServiceItemRepository
{
    Task<ServiceItem?> GetByIdAsync(int id);
    Task<List<ServiceItem>> GetAllAsync(string? sortByPrice = null);
    Task AddAsync(ServiceItem service);
    Task UpdateAsync(ServiceItem service);
    Task DeleteAsync(ServiceItem service);
    Task SaveChangesAsync();
}