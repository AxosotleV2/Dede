// Areas/Admin/Controllers/ServicesController.cs

using Dede.Domain.Entities;
using Dede.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dede.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ServicesController : Controller
{
    private readonly IServiceCatalogService _serviceCatalog;

    public ServicesController(IServiceCatalogService serviceCatalog)
    {
        _serviceCatalog = serviceCatalog;
    }

    public async Task<IActionResult> Index(string? sort)
    {
        var services = await _serviceCatalog.GetServicesAsync(sort);
        return View(services);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ServiceItem model)
    {
        if (!ModelState.IsValid) return View(model);

        await _serviceCatalog.CreateAsync(model);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var service = await _serviceCatalog.GetByIdAsync(id);
        if (service == null) return NotFound();
        return View(service);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ServiceItem model)
    {
        if (!ModelState.IsValid) return View(model);

        await _serviceCatalog.UpdateAsync(model);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _serviceCatalog.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}