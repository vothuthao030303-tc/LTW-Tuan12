using System.ComponentModel.DataAnnotations;

namespace RealEstateWeb.Models;

public partial class User
{
    public int UserId { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = "";

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [Required, StringLength(20)]
    public string Role { get; set; } = "User";

    [StringLength(20)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
