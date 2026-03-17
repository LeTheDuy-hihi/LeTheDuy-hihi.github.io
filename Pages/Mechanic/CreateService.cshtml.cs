using System.ComponentModel.DataAnnotations;
using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Mechanic;

[Authorize(Roles = "Mechanic")]
public class CreateServiceModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateServiceModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public int VehicleId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập mô tả.")]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    public decimal TotalCost { get; set; }

    public Vehicle? Vehicle { get; set; }
    public IList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public IList<Service> Services { get; set; } = new List<Service>();
    public IList<Part> Parts { get; set; } = new List<Part>();

    public async Task<IActionResult> OnGetAsync(int? vehicleId)
    {
        if (!vehicleId.HasValue || vehicleId.Value <= 0)
        {
            Vehicles = await _context.Vehicles
                .Include(v => v.Customer)
                .OrderBy(v => v.LicensePlate)
                .ToListAsync();

            return Page();
        }

        VehicleId = vehicleId.Value;
        Vehicle = await _context.Vehicles
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == VehicleId);

        if (Vehicle == null)
        {
            return NotFound();
        }

        Services = await _context.Services.ToListAsync();
        Parts = await _context.Parts.Where(p => p.Quantity > 0).ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int[] selectedServices, int[] partIds, int[] quantities)
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        var serviceHistory = new ServiceHistory
        {
            VehicleId = VehicleId,
            Description = Description,
            TotalCost = TotalCost,
            ServiceDate = DateTime.Now
        };

        // Set mechanic if user is mechanic
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser != null && await _userManager.IsInRoleAsync(currentUser, "Mechanic"))
        {
            serviceHistory.MechanicId = currentUser.Id;
        }

        if (VehicleId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Vehicle must be selected.");
            await LoadDataAsync();
            return Page();
        }

        _context.ServiceHistories.Add(serviceHistory);
        await _context.SaveChangesAsync();

        // Add services
        if (selectedServices != null)
        {
            foreach (var serviceId in selectedServices)
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service != null)
                {
                    var serviceHistoryService = new ServiceHistoryService
                    {
                        ServiceHistoryId = serviceHistory.Id,
                        ServiceId = serviceId,
                        Price = service.Price
                    };
                    _context.ServiceHistoryServices.Add(serviceHistoryService);
                }
            }
        }

        // Add parts and deduct inventory
        if (partIds != null && quantities != null)
        {
            for (int i = 0; i < partIds.Length; i++)
            {
                var partId = partIds[i];
                var quantity = quantities[i];

                var part = await _context.Parts.FindAsync(partId);
                if (part != null && part.Quantity >= quantity)
                {
                    var serviceHistoryPart = new ServiceHistoryPart
                    {
                        ServiceHistoryId = serviceHistory.Id,
                        PartId = partId,
                        Quantity = quantity,
                        UnitPrice = part.Price
                    };
                    _context.ServiceHistoryParts.Add(serviceHistoryPart);

                    // Deduct inventory
                    part.Quantity -= quantity;
                }
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToPage("/Mechanic/ServiceHistory", new { vehicleId = serviceHistory.VehicleId });
    }

    private async Task LoadDataAsync()
    {
        Vehicle = await _context.Vehicles
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == VehicleId);

        Services = await _context.Services.ToListAsync();
        Parts = await _context.Parts.Where(p => p.Quantity > 0).ToListAsync();
    }
}
