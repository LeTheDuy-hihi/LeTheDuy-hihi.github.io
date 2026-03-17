using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Accountant;

[Authorize(Roles = "Accountant")]
public class ReportsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ReportsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public decimal MonthlyRevenue { get; set; }
    public int TotalInvoices { get; set; }
    public int NewCustomers { get; set; }
    public List<string> ChartLabels { get; set; } = new();
    public List<decimal> ChartData { get; set; } = new();

    public async Task OnGetAsync()
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // Monthly revenue
        MonthlyRevenue = (decimal)await _context.ServiceHistories
    .Where(sh => sh.ServiceDate >= startOfMonth)
    .Select(sh => (double)sh.TotalCost)
    .SumAsync();
        // Total invoices this month
        TotalInvoices = await _context.ServiceHistories
            .CountAsync(sh => sh.ServiceDate >= startOfMonth);

        // New customers this month
        NewCustomers = await _context.Customers
            .CountAsync(c => c.CreatedAt >= startOfMonth);

        // Generate chart data for last 12 months
        for (int i = 11; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);

            var revenue = (decimal)await _context.ServiceHistories
    .Where(sh => sh.ServiceDate >= monthStart && sh.ServiceDate < monthEnd)
    .Select(sh => (double)sh.TotalCost)
    .SumAsync();
            ChartLabels.Add(monthStart.ToString("MM/yyyy"));
            ChartData.Add(revenue);
        }
    }
}
