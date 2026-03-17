using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Admin;

[Authorize(Roles = "Admin")]
public class PartsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public PartsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Part> Parts { get; set; } = new List<Part>();

    [BindProperty]
    public Part InputPart { get; set; } = new Part();

    public bool IsEditing { get; set; }

    public async Task OnGetAsync(int? editId, bool create)
    {
        Parts = await _context.Parts.OrderBy(p => p.Name).ToListAsync();

        if (create)
        {
            InputPart = new Part();
            IsEditing = true;
        }
        else if (editId.HasValue)
        {
            var part = await _context.Parts.FindAsync(editId.Value);
            if (part != null)
            {
                InputPart = part;
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

        _context.Parts.Add(InputPart);
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

        var part = await _context.Parts.FindAsync(id);
        if (part == null)
        {
            return NotFound();
        }

        part.Name = InputPart.Name;
        part.Description = InputPart.Description;
        part.Price = InputPart.Price;
        part.Quantity = InputPart.Quantity;
        part.Category = InputPart.Category;
        part.Supplier = InputPart.Supplier;

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var part = await _context.Parts.FindAsync(id);
        if (part != null)
        {
            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
