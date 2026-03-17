using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ReportsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ReportsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public decimal RevenueThisMonth { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public List<(string LicensePlate, int Count)> TopVehicles { get; set; } = new();

    public async Task OnGetAsync()
    {
        var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        RevenueThisMonth = (decimal)await _context.ServiceHistories
    .Where(sh => sh.ServiceDate >= startOfMonth)
    .Select(sh => (double)sh.TotalCost)
    .SumAsync();
        NewCustomersThisMonth = await _context.Customers
            .CountAsync(c => c.CreatedAt >= startOfMonth);

        var topVehiclesTemp = await _context.ServiceHistories
            .Where(sh => sh.ServiceDate >= startOfMonth)
            .GroupBy(sh => sh.VehicleId)
            .Select(g => new { VehicleId = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(3)
            .Join(_context.Vehicles, g => g.VehicleId, v => v.Id, (g, v) => new { v.LicensePlate, g.Count })
            .ToListAsync();

        TopVehicles = topVehiclesTemp
            .Select(x => (x.LicensePlate, x.Count))
            .ToList();
    }
}
