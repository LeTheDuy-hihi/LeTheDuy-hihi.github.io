using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ServicesModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ServicesModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Service> Services { get; set; } = new List<Service>();

    [BindProperty]
    public Service InputService { get; set; } = new Service();

    public bool IsEditing { get; set; }

    public async Task OnGetAsync(int? editId, bool create)
    {
        Services = await _context.Services.OrderBy(s => s.Name).ToListAsync();

        if (create)
        {
            InputService = new Service();
            IsEditing = true;
        }
        else if (editId.HasValue)
        {
            var service = await _context.Services.FindAsync(editId.Value);
            if (service != null)
            {
                InputService = service;
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

        _context.Services.Add(InputService);
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

        var service = await _context.Services.FindAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        service.Name = InputService.Name;
        service.Description = InputService.Description;
        service.Price = InputService.Price;
        service.EstimatedDuration = InputService.EstimatedDuration;

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service != null)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
