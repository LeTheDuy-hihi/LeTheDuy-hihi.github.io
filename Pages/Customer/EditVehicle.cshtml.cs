using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Customer;

[Authorize(Roles = "Customer")]
public class EditVehicleModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EditVehicleModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public Vehicle Vehicle { get; set; } = new Vehicle();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Forbid();
        }

        var customer = await _context.Customers
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Email == user.Email);

        if (customer == null)
        {
            return NotFound();
        }

        var vehicle = customer.Vehicles?.FirstOrDefault(v => v.Id == id);
        if (vehicle == null)
        {
            return NotFound();
        }

        Vehicle = vehicle;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Forbid();
        }

        var customer = await _context.Customers
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Email == user.Email);

        if (customer == null)
        {
            return NotFound();
        }

        var vehicle = customer.Vehicles.FirstOrDefault(v => v.Id == Vehicle.Id);
        if (vehicle == null)
        {
            return NotFound();
        }

        // Update vehicle properties
        vehicle.Make = Vehicle.Make;
        vehicle.Model = Vehicle.Model;
        vehicle.Year = Vehicle.Year;
        vehicle.LicensePlate = Vehicle.LicensePlate;
        vehicle.Mileage = Vehicle.Mileage;

        await _context.SaveChangesAsync();

        return RedirectToPage("./Vehicles");
    }
}
