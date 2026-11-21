using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RealEstateWeb.Areas.Admin.Controllers;

[Area("Admin")]
[AllowAnonymous]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Properties", new { area = "Admin" });
    }
}
