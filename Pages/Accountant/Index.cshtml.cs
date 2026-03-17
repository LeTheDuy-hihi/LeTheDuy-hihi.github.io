using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Accountant;

[Authorize(Roles = "Accountant")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public decimal MonthlyRevenue { get; set; }
    public int TodayInvoices { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalParts { get; set; }
    public IList<ServiceHistory> RecentInvoices { get; set; } = new List<ServiceHistory>();
    public IList<Part> LowStockParts { get; set; } = new List<Part>();

    public async Task OnGetAsync()
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // Monthly revenue
        MonthlyRevenue = (decimal)await _context.ServiceHistories
    .Where(sh => sh.ServiceDate >= startOfMonth)
    .Select(sh => (double)sh.TotalCost)
    .SumAsync();
        // Today's invoices
        TodayInvoices = await _context.ServiceHistories
            .CountAsync(sh => sh.ServiceDate.Date == now.Date);

        // Total customers
        TotalCustomers = await _context.Customers.CountAsync();

        // Total parts in stock
        TotalParts = await _context.Parts.SumAsync(p => p.Quantity);

        // Recent invoices
        RecentInvoices = await _context.ServiceHistories
            .Include(sh => sh.Vehicle)
            .OrderByDescending(sh => sh.ServiceDate)
            .Take(5)
            .ToListAsync();

        // Low stock parts (less than 10)
        LowStockParts = await _context.Parts
            .Where(p => p.Quantity < 10)
            .OrderBy(p => p.Quantity)
            .Take(5)
            .ToListAsync();
    }
}
