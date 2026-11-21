using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Models;
using RealEstateWeb.Services;
using RealEstateWeb.ViewModels;

namespace RealEstateWeb.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class PropertyImagesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly PropertyImageStorage _storage;
    public PropertyImagesController(ApplicationDbContext db, PropertyImageStorage storage) { _db = db; _storage = storage; }

    public async Task<IActionResult> Index(int? propertyId)
    {
        await Lists(propertyId);
        var query = _db.PropertyImages.AsNoTracking().Include(x => x.Property).AsQueryable();
        if (propertyId.HasValue) query = query.Where(x => x.PropertyId == propertyId);
        return View(await query.OrderByDescending(x => x.PropertyImageId).ToListAsync());
    }

    public async Task<IActionResult> Create(int? propertyId)
    {
        await Lists(propertyId);
        return View(new PropertyImageViewModel { PropertyId = propertyId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Create(PropertyImageViewModel model)
    {
        var property = await _db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.PropertyId == model.PropertyId);
        if (property == null) ModelState.AddModelError("PropertyId", "Bất động sản không tồn tại.");
        var url = await ValidateImage(model);
        if (!ModelState.IsValid) { await Lists(model.PropertyId); return View(model); }
        var image = new PropertyImage { PropertyId = model.PropertyId, ImageUrl = url!, IsMain = model.IsMain };
        if (model.IsMain)
        {
            foreach (var other in property!.Images) other.IsMain = false;
            property.MainImage = image.ImageUrl;
            property.UpdatedAt = DateTime.Now;
        }
        _db.PropertyImages.Add(image);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm hình ảnh.";
        return RedirectToAction(nameof(Index), new { propertyId = model.PropertyId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var image = await _db.PropertyImages.FindAsync(id);
        if (image == null) return NotFound();
        await Lists(image.PropertyId);
        return View(new PropertyImageViewModel { PropertyImageId = id, PropertyId = image.PropertyId, ImageUrl = image.ImageUrl, IsMain = image.IsMain });
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Edit(int id, PropertyImageViewModel model)
    {
        if (id != model.PropertyImageId) return BadRequest();
        var image = await _db.PropertyImages.Include(x => x.Property).ThenInclude(x => x!.Images).FirstOrDefaultAsync(x => x.PropertyImageId == id);
        if (image == null) return NotFound();
        if (model.PropertyId != image.PropertyId) ModelState.AddModelError("PropertyId", "Không thể chuyển ảnh sang tin khác. Hãy tạo ảnh mới.");
        var url = await ValidateImage(model);
        if (!ModelState.IsValid) { await Lists(image.PropertyId); return View(model); }
        var property = image.Property!;
        var wasMain = image.IsMain || property.MainImage == image.ImageUrl;
        image.ImageUrl = url!;
        image.IsMain = model.IsMain;
        if (model.IsMain)
        {
            foreach (var other in property.Images.Where(x => x.PropertyImageId != id)) other.IsMain = false;
            property.MainImage = image.ImageUrl;
        }
        else if (wasMain)
        {
            var replacement = property.Images.Where(x => x.PropertyImageId != id).OrderByDescending(x => x.IsMain).ThenBy(x => x.PropertyImageId).FirstOrDefault();
            foreach (var other in property.Images.Where(x => x.PropertyImageId != id)) other.IsMain = other == replacement;
            property.MainImage = replacement?.ImageUrl ?? "/user/images/hotel_feture_1.jpg";
        }
        property.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật hình ảnh.";
        return RedirectToAction(nameof(Index), new { propertyId = image.PropertyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var image = await _db.PropertyImages.Include(x => x.Property).ThenInclude(x => x!.Images).FirstOrDefaultAsync(x => x.PropertyImageId == id);
        if (image == null) return NotFound();
        var property = image.Property!;
        if (image.IsMain || property.MainImage == image.ImageUrl)
        {
            var replacement = property.Images.Where(x => x.PropertyImageId != id).OrderByDescending(x => x.IsMain).ThenBy(x => x.PropertyImageId).FirstOrDefault();
            foreach (var other in property.Images.Where(x => x.PropertyImageId != id)) other.IsMain = other == replacement;
            property.MainImage = replacement?.ImageUrl ?? "/user/images/hotel_feture_1.jpg";
            property.UpdatedAt = DateTime.Now;
        }
        _db.PropertyImages.Remove(image);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa bản ghi ảnh.";
        return RedirectToAction(nameof(Index), new { propertyId = image.PropertyId });
    }

    private async Task Lists(int? id) => ViewBag.PropertyId = new SelectList(
        await _db.Properties.AsNoTracking().OrderBy(x => x.Title).ToListAsync(), "PropertyId", "Title", id);

    private async Task<string?> ValidateImage(PropertyImageViewModel model)
    {
        if (!ModelState.IsValid) return null;
        try
        {
            var url = await _storage.SaveAsync(model.Image) ?? model.ImageUrl;
            if (!_storage.Exists(url)) ModelState.AddModelError("ImageUrl", "Tải ảnh lên hoặc nhập đường dẫn ảnh có sẵn trong project.");
            return url;
        }
        catch (InvalidOperationException error) { ModelState.AddModelError("Image", error.Message); return null; }
    }
}
