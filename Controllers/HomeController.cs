using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RealEstateWeb.Models;

namespace RealEstateWeb.Controllers;

public class HomeController : Controller
{
    private readonly IOptionsSnapshot<TeamOptions> _team;

    public HomeController(IOptionsSnapshot<TeamOptions> team)
    {
        _team = team;
    }

    public IActionResult Index()
    {
        ViewBag.Team = _team.Value;
        return View();
    }

    public IActionResult Error() => Problem("Có lỗi khi xử lý yêu cầu. Vui lòng thử lại.");
}
