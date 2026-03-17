using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoGarageManager.Pages.Admin;

[Authorize(Roles = "Admin")]
public class PermissionsModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionsModel(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public IList<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();

    public async Task OnGetAsync()
    {
        var roles = _roleManager!.Roles.ToList();

        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            Roles.Add(new RoleViewModel
            {
                Name = role.Name,
                UserCount = usersInRole.Count,
                Users = usersInRole.Select(u => new UserViewModel
                {
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty
                }).ToList()
            });
        }
    }
}

public class RoleViewModel
{
    public string Name { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
}

public class UserViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
