using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Models;

namespace RealEstateWeb.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class PropertyTypesController : Controller
{
    private readonly ApplicationDbContext _db;
    public PropertyTypesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() => View(await _db.PropertyTypes.AsNoTracking().OrderBy(x => x.TypeName).ToListAsync());
    public IActionResult Create() => View(new PropertyType());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TypeName,Description")] PropertyType model)
    {
        await ValidateType(model);
        if (!ModelState.IsValid) return View(model);
        _db.PropertyTypes.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm loại bất động sản.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _db.PropertyTypes.FindAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("PropertyTypeId,TypeName,Description")] PropertyType model)
    {
        if (id != model.PropertyTypeId) return BadRequest();
        var existing = await _db.PropertyTypes.FindAsync(id);
        if (existing == null) return NotFound();
        await ValidateType(model);
        if (!ModelState.IsValid) return View(model);
        existing.TypeName = model.TypeName; existing.Description = model.Description;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật loại bất động sản.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _db.PropertyTypes.FindAsync(id);
        if (model == null) return NotFound();
        if (await _db.Properties.AnyAsync(x => x.PropertyTypeId == id))
        {
            TempData["Error"] = "Không thể xóa loại đang có bất động sản.";
            return RedirectToAction(nameof(Index));
        }
        _db.PropertyTypes.Remove(model);
        try { await _db.SaveChangesAsync(); TempData["Success"] = "Đã xóa loại bất động sản."; }
        catch (DbUpdateException) { TempData["Error"] = "Loại bất động sản đang được sử dụng, không thể xóa."; }
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateType(PropertyType model)
    {
        model.TypeName = (model.TypeName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(model.TypeName)) ModelState.AddModelError("TypeName", "Vui lòng nhập tên loại.");
        if (await _db.PropertyTypes.AnyAsync(x => x.PropertyTypeId != model.PropertyTypeId && x.TypeName == model.TypeName))
            ModelState.AddModelError("TypeName", "Tên loại đã tồn tại.");
    }
}
