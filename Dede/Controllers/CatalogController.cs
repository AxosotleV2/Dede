using Dede.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Dede.Controllers;

[Authorize]  // <- ДОБАВИЛИ
public class CatalogController : Controller
{
    private readonly IServiceCatalogService _serviceCatalog;

    public CatalogController(IServiceCatalogService serviceCatalog)
    {
        _serviceCatalog = serviceCatalog;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Service(int id)
    {
        var service = await _serviceCatalog.GetByIdAsync(id);
        if (service == null)
            return NotFound();

        // Обычный пользователь не видит выключенные услуги
        if (!service.IsActive && !User.IsInRole("Admin"))
            return NotFound();

        return View(service);
    }
}