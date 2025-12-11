using Dede.Domain.Entities;
using Dede.Domain.Interfaces;

namespace Dede.Service.Services;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceItemRepository _serviceRepo;

    public ServiceCatalogService(IServiceItemRepository serviceRepo)
    {
        _serviceRepo = serviceRepo;
    }

    public Task<List<ServiceItem>> GetServicesAsync(string? sort)
    {
        return _serviceRepo.GetAllAsync(sort);
    }

    public Task<ServiceItem?> GetByIdAsync(int id)
    {
        return _serviceRepo.GetByIdAsync(id);
    }

    public async Task<ServiceItem> CreateAsync(ServiceItem service)
    {
        await _serviceRepo.AddAsync(service);
        await _serviceRepo.SaveChangesAsync();
        return service;
    }

    public async Task<ServiceItem?> UpdateAsync(ServiceItem service)
    {
        var existing = await _serviceRepo.GetByIdAsync(service.Id);
        if (existing == null) return null;

        existing.Name = service.Name;
        existing.Description = service.Description;
        existing.MinPrice = service.MinPrice;
        existing.Category = service.Category;
        existing.Icon = service.Icon;
        existing.IsActive = service.IsActive;

        await _serviceRepo.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _serviceRepo.GetByIdAsync(id);
        if (existing == null) return false;

        await _serviceRepo.DeleteAsync(existing);
        await _serviceRepo.SaveChangesAsync();
        return true;
    }
}