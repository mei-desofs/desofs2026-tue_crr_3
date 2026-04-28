namespace TeaShop.Domain.Users;

public static class Roles
{
    public const string Admin = "ADMIN";
    public const string Manager = "MANAGER";
    public const string Customer = "CUSTOMER";

 
    public static readonly IReadOnlySet<string> All =
        new HashSet<string> { Admin, Manager, Customer };

    public static bool IsValid(string? role) =>
        !string.IsNullOrWhiteSpace(role) && All.Contains(role.ToUpperInvariant());
}