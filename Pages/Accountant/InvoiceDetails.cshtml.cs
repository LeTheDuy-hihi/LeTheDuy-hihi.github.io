using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Accountant;

[Authorize(Roles = "Accountant")]
public class InvoiceDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public InvoiceDetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public ServiceHistory? Invoice { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Invoice = await _context!.ServiceHistories
            .Include(sh => sh.Vehicle)
                .ThenInclude(v => v!.Customer)
            .Include(sh => sh.ServicesPerformed)
                .ThenInclude(shs => shs!.Service)
            .Include(sh => sh.PartsUsed)
                .ThenInclude(shp => shp!.Part)
            .FirstOrDefaultAsync(sh => sh.Id == id);

        if (Invoice == null)
        {
            return NotFound();
        }

        return Page();
    }
}
