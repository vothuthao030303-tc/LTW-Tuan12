using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;
using RealEstateWeb.Models;
using RealEstateWeb.ViewModels;

namespace RealEstateWeb.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;
    public AccountController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null || user.Status != "Active" ||
            new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng, hoặc tài khoản đã ngừng hoạt động.");
            return View(model);
        }
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = model.RememberMe });
        if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl!);
        return user.Role == "Admin" ? RedirectToAction("Index", "Home", new { area = "Admin" })
            : RedirectToAction("Index", "Home", new { area = "" });
    }

    [HttpGet] public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(x => x.Email == email)) ModelState.AddModelError("Email", "Email đã được sử dụng.");
        if (!ModelState.IsValid) return View(model);
        var user = new User { FullName = model.FullName.Trim(), Email = email, Phone = model.Phone, Role = "User" };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, model.Password);
        _db.Users.Add(user);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể tạo tài khoản. Kiểm tra email có bị trùng.");
            return View(model);
        }
        TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    public IActionResult AccessDenied() => View();
}
