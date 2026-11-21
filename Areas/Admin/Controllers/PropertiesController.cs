using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Models;
using RealEstateWeb.Services;
using RealEstateWeb.ViewModels;

namespace RealEstateWeb.Areas.Admin.Controllers;

[Area("Admin")]
[AllowAnonymous]
public class PropertiesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly PropertyImageStorage _storage;

    public PropertiesController(ApplicationDbContext db, PropertyImageStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    // READ: search by title/address, filter status and paginate 10 records per page.
    public async Task<IActionResult> Index(string? keyword, string? status, int page = 1)
    {
        const int pageSize = 10;
        var query = _db.Properties.AsNoTracking()
            .Include(property => property.PropertyType)
            .Include(property => property.Location)
            .Include(property => property.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();
            query = query.Where(property => property.Title.Contains(keyword) || property.Address.Contains(keyword));
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(property => property.Status == status);
        }

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        return View(new AdminPropertyListViewModel
        {
            Items = await query.OrderByDescending(property => property.CreatedAt)
                .ThenByDescending(property => property.PropertyId)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(),
            Keyword = keyword,
            Status = status,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        });
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var property = await DetailsQuery().FirstOrDefaultAsync(item => item.PropertyId == id);
        return property == null ? NotFound() : View(property);
    }

    public async Task<IActionResult> Create()
    {
        await Lists();
        return View(new Property { Status = "Approved" });
    }

    // CREATE: individual coursework without login. Ignore posted Status and publish on save.
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,Price,Area,Address,PropertyTypeId,LocationId,UserId,Bedrooms,Bathrooms,IsFeatured")] Property model,
        IFormFile? image)
    {
        model.Status = "Approved";
        ModelState.Remove(nameof(Property.Status));
        await ValidateProperty(model);
        var url = await ReadImage(image);

        if (!ModelState.IsValid)
        {
            await Lists();
            return View(model);
        }

        model.MainImage = url ?? "/user/images/hotel_feture_1.jpg";
        model.Images.Add(new PropertyImage { ImageUrl = model.MainImage, IsMain = true });
        _db.Properties.Add(model);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể lưu tin. Danh mục, khu vực hoặc người liên hệ có thể đã thay đổi. Vui lòng kiểm tra và thử lại.");
            await Lists();
            return View(model);
        }

        TempData["Success"] = "Đã thêm và duyệt bất động sản. Tin đã được hiển thị trên website.";
        return RedirectToAction(nameof(Details), new { id = model.PropertyId });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var model = await _db.Properties.FindAsync(id.Value);
        if (model == null)
        {
            return NotFound();
        }
        await Lists();
        return View(model);
    }

    // UPDATE: load the tracked entity and update only editable fields.
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Edit(int id,
        [Bind("PropertyId,Title,Description,Price,Area,Address,PropertyTypeId,LocationId,UserId,Bedrooms,Bathrooms,Status,IsFeatured")] Property model,
        IFormFile? image)
    {
        if (id != model.PropertyId)
        {
            return BadRequest();
        }
        var existing = await _db.Properties.Include(property => property.Images)
            .FirstOrDefaultAsync(property => property.PropertyId == id);
        if (existing == null)
        {
            return NotFound();
        }

        await ValidateProperty(model);
        var url = await ReadImage(image);
        if (!ModelState.IsValid)
        {
            model.MainImage = existing.MainImage;
            await Lists();
            return View(model);
        }

        existing.Title = model.Title;
        existing.Description = model.Description;
        existing.Price = model.Price;
        existing.Area = model.Area;
        existing.Address = model.Address;
        existing.PropertyTypeId = model.PropertyTypeId;
        existing.LocationId = model.LocationId;
        existing.UserId = model.UserId;
        existing.Bedrooms = model.Bedrooms;
        existing.Bathrooms = model.Bathrooms;
        existing.Status = model.Status;
        existing.IsFeatured = model.IsFeatured;
        existing.UpdatedAt = DateTime.Now;
        if (url != null)
        {
            foreach (var oldImage in existing.Images)
            {
                oldImage.IsMain = false;
            }
            existing.MainImage = url;
            existing.Images.Add(new PropertyImage { ImageUrl = url, IsMain = true });
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Properties.AnyAsync(property => property.PropertyId == id))
            {
                return NotFound();
            }
            ModelState.AddModelError("", "Dữ liệu đã thay đổi. Vui lòng tải lại trang và thử lại.");
            model.MainImage = existing.MainImage;
            await Lists();
            return View(model);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể cập nhật tin. Vui lòng kiểm tra danh mục, khu vực và người liên hệ.");
            model.MainImage = existing.MainImage;
            await Lists();
            return View(model);
        }

        TempData["Success"] = "Đã cập nhật bất động sản.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET displays confirmation only, including listings not approved for the public site.
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var property = await DetailsQuery().FirstOrDefaultAsync(item => item.PropertyId == id);
        return property == null ? NotFound() : View(property);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var property = await _db.Properties.FindAsync(id);
        if (property == null)
        {
            return NotFound();
        }
        _db.Properties.Remove(property);
        try
        {
            // The existing SQL foreign key deletes dependent PropertyImages records.
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa bất động sản và các bản ghi hình ảnh liên quan.";
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Tin đã bị thay đổi hoặc xóa bởi yêu cầu khác. Vui lòng kiểm tra lại danh sách.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Chưa xóa được tin. Vui lòng kiểm tra dữ liệu liên quan và thử lại.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Approve(int id) => ChangeStatus(id, "Approved");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(int id) => ChangeStatus(id, "Rejected");

    private async Task<IActionResult> ChangeStatus(int id, string status)
    {
        var property = await _db.Properties.FindAsync(id);
        if (property == null)
        {
            return NotFound();
        }
        property.Status = status;
        property.UpdatedAt = DateTime.Now;
        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật trạng thái tin.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể cập nhật trạng thái. Vui lòng tải lại danh sách và thử lại.";
        }
        return RedirectToAction(nameof(Index));
    }

    private IQueryable<Property> DetailsQuery() => _db.Properties.AsNoTracking()
        .Include(property => property.PropertyType)
        .Include(property => property.Location)
        .Include(property => property.User)
        .Include(property => property.Images);

    private async Task<string?> ReadImage(IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            return null;
        }
        try
        {
            return await _storage.SaveAsync(image);
        }
        catch (InvalidOperationException error)
        {
            ModelState.AddModelError("", error.Message);
            return null;
        }
    }

    private async Task Lists()
    {
        ViewBag.PropertyTypeId = new SelectList(await _db.PropertyTypes.AsNoTracking()
            .OrderBy(item => item.TypeName).ToListAsync(), "PropertyTypeId", "TypeName");
        ViewBag.LocationId = new SelectList(await _db.Locations.AsNoTracking()
            .OrderBy(item => item.City).ThenBy(item => item.District)
            .Select(item => new { item.LocationId, Name = item.City + " - " + item.District }).ToListAsync(), "LocationId", "Name");
        ViewBag.UserId = new SelectList(await _db.Users.AsNoTracking()
            .Where(item => item.Role == "User").OrderBy(item => item.FullName).ToListAsync(), "UserId", "FullName");
    }

    private async Task ValidateProperty(Property model)
    {
        if (!new[] { "Pending", "Approved", "Rejected", "Sold" }.Contains(model.Status))
        {
            ModelState.AddModelError(nameof(Property.Status), "Trạng thái không hợp lệ.");
        }
        if (!await _db.PropertyTypes.AnyAsync(item => item.PropertyTypeId == model.PropertyTypeId))
        {
            ModelState.AddModelError(nameof(Property.PropertyTypeId), "Vui lòng chọn loại bất động sản.");
        }
        if (!await _db.Locations.AnyAsync(item => item.LocationId == model.LocationId))
        {
            ModelState.AddModelError(nameof(Property.LocationId), "Vui lòng chọn khu vực.");
        }
        if (!await _db.Users.AnyAsync(item => item.UserId == model.UserId && item.Role == "User"))
        {
            ModelState.AddModelError(nameof(Property.UserId), "Vui lòng chọn người liên hệ có vai trò User.");
        }
    }
}
