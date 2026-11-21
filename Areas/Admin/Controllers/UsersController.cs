using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Models;
using RealEstateWeb.ViewModels;

namespace RealEstateWeb.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _db;
    public UsersController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? keyword)
    {
        var query = _db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x => x.FullName.Contains(keyword) || x.Email.Contains(keyword));
        ViewBag.Keyword = keyword;
        return View(await query.OrderBy(x => x.UserId).ToListAsync());
    }

    public IActionResult Create() => View(new AdminUserViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserViewModel model)
    {
        await ValidateUser(model);
        if (string.IsNullOrWhiteSpace(model.NewPassword)) ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu.");
        if (!ModelState.IsValid) return View(model);
        var user = new User { FullName = model.FullName, Email = model.Email, Phone = model.Phone,
            Address = model.Address, Role = model.Role, Status = model.Status };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, model.NewPassword!);
        _db.Users.Add(user);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể lưu tài khoản. Kiểm tra email có bị trùng.");
            return View(model);
        }
        TempData["Success"] = "Đã tạo tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return View(new AdminUserViewModel { UserId = user.UserId, FullName = user.FullName,
            Email = user.Email, Phone = user.Phone, Address = user.Address, Role = user.Role, Status = user.Status });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminUserViewModel model)
    {
        if (id != model.UserId) return BadRequest();
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        await ValidateUser(model);
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString() && (model.Role != "Admin" || model.Status != "Active"))
            ModelState.AddModelError("", "Không thể tự khóa hoặc hạ quyền tài khoản đang đăng nhập.");
        if (!ModelState.IsValid) return View(model);
        user.FullName = model.FullName; user.Email = model.Email; user.Phone = model.Phone;
        user.Address = model.Address; user.Role = model.Role; user.Status = model.Status;
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, model.NewPassword);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể cập nhật. Kiểm tra email có bị trùng.");
            return View(model);
        }
        TempData["Success"] = "Đã cập nhật tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString())
            TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập.";
        else if (await _db.Properties.AnyAsync(x => x.UserId == id))
            TempData["Error"] = "Tài khoản đang có tin đăng. Hãy khóa tài khoản hoặc chuyển tin trước khi xóa.";
        else
        {
            _db.Users.Remove(user);
            try { await _db.SaveChangesAsync(); TempData["Success"] = "Đã xóa tài khoản."; }
            catch (DbUpdateException) { TempData["Error"] = "Tài khoản đang được sử dụng, không thể xóa."; }
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateUser(AdminUserViewModel model)
    {
        model.FullName = (model.FullName ?? "").Trim();
        model.Email = (model.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(model.FullName)) ModelState.AddModelError("FullName", "Vui lòng nhập họ tên.");
        if (await _db.Users.AnyAsync(x => x.UserId != model.UserId && x.Email == model.Email))
            ModelState.AddModelError("Email", "Email đã được sử dụng.");
    }
}
