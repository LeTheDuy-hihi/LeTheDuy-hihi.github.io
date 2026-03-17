using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Accountant;

[Authorize(Roles = "Accountant")]
public class InvoicesModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public InvoicesModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<ServiceHistory> Invoices { get; set; } = new List<ServiceHistory>();

    public async Task OnGetAsync(string? date)
    {
        var query = _context!.ServiceHistories
            .Include(sh => sh.Vehicle)
                .ThenInclude(v => v!.Customer)
            .AsQueryable();

        if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var filterDate))
        {
            query = query.Where(sh => sh.ServiceDate.Date == filterDate.Date);
        }

        Invoices = await query
            .OrderByDescending(sh => sh.ServiceDate)
            .ToListAsync();
    }
}
