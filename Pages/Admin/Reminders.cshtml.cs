using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Admin;

[Authorize(Roles = "Admin")]
public class RemindersModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public RemindersModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<MaintenanceReminder> Reminders { get; set; } = new List<MaintenanceReminder>();
    public IList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    [BindProperty]
    public MaintenanceReminder InputReminder { get; set; } = new MaintenanceReminder();

    public bool IsEditing { get; set; }

    public async Task OnGetAsync(int? editId, bool create)
    {
        Vehicles = await _context!.Vehicles.Include(v => v.Customer).ToListAsync();
        Reminders = await _context.MaintenanceReminders
            .Include(r => r.Vehicle)
            .ThenInclude(v => v!.Customer)
            .OrderBy(r => r.ReminderDate)
            .ToListAsync();

        if (create)
        {
            InputReminder = new MaintenanceReminder { ReminderDate = DateTime.Today };
            IsEditing = true;
        }
        else if (editId.HasValue)
        {
            var reminder = await _context.MaintenanceReminders.FindAsync(editId.Value);
            if (reminder != null)
            {
                InputReminder = reminder;
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

        _context.MaintenanceReminders.Add(InputReminder);
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync(id, false);
            IsEditing = true;
            return Page();
        }

        var reminder = await _context.MaintenanceReminders.FindAsync(id);
        if (reminder == null)
        {
            return NotFound();
        }

        reminder.VehicleId = InputReminder.VehicleId;
        reminder.Title = InputReminder.Title;
        reminder.Message = InputReminder.Message;
        reminder.ReminderDate = InputReminder.ReminderDate;
        reminder.IsCompleted = InputReminder.IsCompleted;

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var reminder = await _context.MaintenanceReminders.FindAsync(id);
        if (reminder != null)
        {
            _context.MaintenanceReminders.Remove(reminder);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
