using AutoGarageManager.Data;
using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Accountant;

[Authorize(Roles = "Accountant")]
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

    [BindProperty]
    public int RestockQuantity { get; set; } = 1;

    public bool IsCreating { get; set; }
    public bool IsEditing { get; set; }
    public bool IsRestocking { get; set; }

    public Part? SelectedPart { get; set; }

    public async Task OnGetAsync(int? create, int? editId, int? restockId)
    {
        Parts = await _context.Parts
            .OrderBy(p => p.Name)
            .ToListAsync();

        IsCreating = create == 1;

        if (editId.HasValue)
        {
            IsEditing = true;
            SelectedPart = await _context.Parts.FindAsync(editId.Value);
            if (SelectedPart != null)
            {
                InputPart = new Part
                {
                    Id = SelectedPart.Id,
                    Name = SelectedPart.Name,
                    Category = SelectedPart.Category,
                    Quantity = SelectedPart.Quantity,
                    Price = SelectedPart.Price,
                    Supplier = SelectedPart.Supplier,
                    Description = SelectedPart.Description
                };
            }
        }

        if (restockId.HasValue)
        {
            IsRestocking = true;
            SelectedPart = await _context.Parts.FindAsync(restockId.Value);
        }
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync(1, null, null);
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
            await OnGetAsync(null, id, null);
            return Page();
        }

        var part = await _context.Parts.FindAsync(id);
        if (part == null) return NotFound();

        part.Name = InputPart.Name;
        part.Category = InputPart.Category;
        part.Quantity = InputPart.Quantity;
        part.Price = InputPart.Price;
        part.Supplier = InputPart.Supplier;
        part.Description = InputPart.Description;

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRestockAsync(int id)
    {
        if (RestockQuantity <= 0)
        {
            ModelState.AddModelError(nameof(RestockQuantity), "Số lượng nhập thêm phải lớn hơn 0.");
            await OnGetAsync(null, null, id);
            return Page();
        }

        var part = await _context.Parts.FindAsync(id);
        if (part == null) return NotFound();

        part.Quantity += RestockQuantity;
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}
