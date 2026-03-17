using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoGarageManager.Pages.Admin;

[Authorize(Roles = "Admin")]
public class EmployeesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public EmployeesModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public IList<string> Roles { get; set; } = new List<string>();

    public Dictionary<string, string> UserRoles { get; set; } = new Dictionary<string, string>();

    [BindProperty]
    public EmployeeInputModel Input { get; set; } = new EmployeeInputModel();

    public bool IsEditing { get; set; }

    public class EmployeeInputModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(string? editId, bool create)
    {
        Users = _userManager.Users.ToList();
        Roles = _roleManager.Roles.Select(r => r.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList();

        UserRoles = new Dictionary<string, string>();
        foreach (var user in Users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            UserRoles[user.Id] = userRoles.FirstOrDefault() ?? string.Empty;
        }

        if (create)
        {
            Input = new EmployeeInputModel();
            IsEditing = true;
        }
        else if (!string.IsNullOrEmpty(editId))
        {
            var user = await _userManager.FindByIdAsync(editId);
            if (user != null)
            {
                Input = new EmployeeInputModel
                {
                    Id = user.Id,
                    FullName = user.FullName ?? string.Empty,
                    Email = user.Email ?? string.Empty
                };
                var userRoles = await _userManager.GetRolesAsync(user);
                Input.Role = userRoles.FirstOrDefault() ?? string.Empty;
                IsEditing = true;
            }
        }
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync(null, false);
            IsEditing = true;
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            FullName = Input.FullName
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await OnGetAsync(null, false);
            IsEditing = true;
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(Input.Role) && await _roleManager.RoleExistsAsync(Input.Role))
        {
            await _userManager.AddToRoleAsync(user, Input.Role);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(string id)
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync(id, false);
            IsEditing = true;
            return Page();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.FullName = Input.FullName;
        user.Email = Input.Email;
        user.UserName = Input.Email;

        var userRoles = await _userManager.GetRolesAsync(user);
        if (!string.IsNullOrEmpty(Input.Role) && await _roleManager.RoleExistsAsync(Input.Role))
        {
            if (!userRoles.Contains(Input.Role))
            {
                await _userManager.AddToRoleAsync(user, Input.Role);
            }
            var removeRoles = userRoles.Where(r => r != Input.Role).ToList();
            if (removeRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, removeRoles);
            }
        }

        if (!string.IsNullOrEmpty(Input.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, Input.Password);
        }

        await _userManager.UpdateAsync(user);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }

        return RedirectToPage();
    }
}
