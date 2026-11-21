using System.ComponentModel.DataAnnotations;

namespace RealEstateWeb.ViewModels;

public class AdminUserViewModel
{
    public int UserId { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập họ tên."), StringLength(100)]
    public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = "";
    [StringLength(20)] public string? Phone { get; set; }
    [StringLength(255)] public string? Address { get; set; }
    [Required, RegularExpression("^(Admin|User)$")]
    public string Role { get; set; } = "User";
    [Required, RegularExpression("^(Active|Inactive|Locked)$")]
    public string Status { get; set; } = "Active";
    [DataType(DataType.Password), StringLength(128, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có 8–128 ký tự.")]
    public string? NewPassword { get; set; }
}
