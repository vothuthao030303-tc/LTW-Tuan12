using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Models;

namespace RealEstateWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class LocationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public LocationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // READ: LINQ is translated to SELECT. Read-only queries do not need tracking.
    public async Task<IActionResult> Index(string? keyword)
    {
        var query = _context.Locations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();
            query = query.Where(location => location.City.Contains(keyword)
                || (location.District != null && location.District.Contains(keyword)));
        }

        ViewBag.Keyword = keyword;
        return View(await query.OrderBy(location => location.City)
            .ThenBy(location => location.District).ToListAsync());
    }

    // Include explicitly loads the related properties; virtual alone does not enable lazy loading.
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var location = await _context.Locations.AsNoTracking()
            .Include(item => item.Properties)
            .FirstOrDefaultAsync(item => item.LocationId == id);

        return location == null ? NotFound() : View(location);
    }

    public IActionResult Create()
    {
        return View(new Location());
    }

    // CREATE: Add marks the entity Added; SaveChangesAsync writes INSERT.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("City,District")] Location location)
    {
        await ValidateLocationAsync(location);

        if (!ModelState.IsValid)
        {
            return View(location);
        }

        _context.Locations.Add(location);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể lưu khu vực. Vui lòng kiểm tra dữ liệu và thử lại.");
            return View(location);
        }

        TempData["Success"] = "Đã thêm khu vực.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var location = await _context.Locations.FindAsync(id.Value);
        return location == null ? NotFound() : View(location);
    }

    // UPDATE: the queried entity is tracked. Assigning fields is enough before SaveChangesAsync.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("LocationId,City,District")] Location location)
    {
        if (id != location.LocationId)
        {
            return BadRequest();
        }

        var existing = await _context.Locations.FindAsync(id);

        if (existing == null)
        {
            return NotFound();
        }

        await ValidateLocationAsync(location);

        if (!ModelState.IsValid)
        {
            return View(location);
        }

        existing.City = location.City;
        existing.District = location.District;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Locations.AnyAsync(item => item.LocationId == id))
            {
                return NotFound();
            }

            ModelState.AddModelError("", "Dữ liệu đã thay đổi. Vui lòng tải lại trang trước khi sửa.");
            return View(location);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể cập nhật khu vực. Vui lòng kiểm tra dữ liệu và thử lại.");
            return View(location);
        }

        TempData["Success"] = "Đã cập nhật khu vực.";
        return RedirectToAction(nameof(Index));
    }

    // GET only displays confirmation; deletion happens exclusively through POST.
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var location = await _context.Locations.AsNoTracking()
            .Include(item => item.Properties)
            .FirstOrDefaultAsync(item => item.LocationId == id);

        return location == null ? NotFound() : View(location);
    }

    // DELETE: Remove marks the entity Deleted; SaveChangesAsync writes DELETE.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location == null)
        {
            return NotFound();
        }

        if (await _context.Properties.AnyAsync(item => item.LocationId == id))
        {
            TempData["Error"] = "Không thể xóa khu vực đang có bất động sản.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        _context.Locations.Remove(location);

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa khu vực.";
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Khu vực đã bị thay đổi hoặc xóa bởi yêu cầu khác. Vui lòng kiểm tra lại danh sách.";
        }
        catch (DbUpdateException)
        {
            // A concurrent request may add a property after the AnyAsync check.
            // SQL Server's foreign key remains the final protection.
            TempData["Error"] = "Không thể xóa khu vực vì dữ liệu đang được sử dụng.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateLocationAsync(Location location)
    {
        location.City = (location.City ?? "").Trim();
        location.District = string.IsNullOrWhiteSpace(location.District) ? null : location.District.Trim();

        if (string.IsNullOrWhiteSpace(location.City))
        {
            ModelState.AddModelError(nameof(Location.City), "Vui lòng nhập tỉnh/thành phố.");
        }

        if (await _context.Locations.AnyAsync(item => item.LocationId != location.LocationId
            && item.City == location.City && item.District == location.District))
        {
            ModelState.AddModelError("", "Khu vực đã tồn tại.");
        }
    }
}
