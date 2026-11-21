namespace RealEstateWeb.Models;

public class TeamOptions
{
    public string ClassName { get; set; } = "";
    public string GroupName { get; set; } = "";
    public List<TeamMember> Members { get; set; } = new();
}

public class TeamMember
{
    public string FullName { get; set; } = "";
    public string StudentId { get; set; } = "";
    public string Responsibility { get; set; } = "";
    public string Controller { get; set; } = "";
}
