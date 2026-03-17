using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Mechanic;

[Authorize(Roles = "Mechanic")]
public class RemindersModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public RemindersModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<MaintenanceReminder> Reminders { get; set; } = new List<MaintenanceReminder>();

    public async Task OnGetAsync()
    {
        Reminders = await _context!.MaintenanceReminders
            .Include(mr => mr.Vehicle)
                .ThenInclude(v => v!.Customer)
            .OrderByDescending(mr => mr.ReminderDate)
            .ToListAsync();
    }
}
