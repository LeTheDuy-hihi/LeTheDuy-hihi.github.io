using AutoGarageManager.Data;
using AutoGarageManager.Models;
using CustomerModel = AutoGarageManager.Models.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Pages.Accountant;

[Authorize(Roles = "Accountant")]
public class CustomersModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CustomersModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<CustomerModel> Customers { get; set; } = new List<CustomerModel>();
    public IList<CustomerTotal> CustomerTotals { get; set; } = new List<CustomerTotal>();

    public bool IsViewing { get; set; }
    public bool IsEditing { get; set; }
    public CustomerModel? SelectedCustomer { get; set; }

    [BindProperty]
    public CustomerModel InputCustomer { get; set; } = new CustomerModel();

    public async Task OnGetAsync(int? viewId, int? editId)
    {
        Customers = await _context.Customers
            .Include(c => c.Vehicles)
            .OrderBy(c => c.FullName)
            .ToListAsync();

        // Calculate total spent by each customer
        var customerIds = Customers.Select(c => c.Id).ToList();
        CustomerTotals = await _context!.ServiceHistories
            .Where(sh => sh.Vehicle != null && customerIds.Contains(sh.Vehicle.CustomerId))
            .GroupBy(sh => sh.Vehicle!.CustomerId)
            .Select(g => new CustomerTotal
            {
                CustomerId = g.Key,
                Total = g.Sum(sh => sh.TotalCost)
            })
            .ToListAsync();

        if (viewId.HasValue)
        {
            IsViewing = true;
            SelectedCustomer = Customers.FirstOrDefault(c => c.Id == viewId.Value);
        }

        if (editId.HasValue)
        {
            IsEditing = true;
            SelectedCustomer = Customers.FirstOrDefault(c => c.Id == editId.Value);
            if (SelectedCustomer != null)
            {
                InputCustomer = new CustomerModel
                {
                    Id = SelectedCustomer.Id,
                    FullName = SelectedCustomer.FullName,
                    Email = SelectedCustomer.Email,
                    Phone = SelectedCustomer.Phone,
                    Address = SelectedCustomer.Address
                };
            }
        }
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync(null, InputCustomer.Id);
            return Page();
        }

        var customer = await _context.Customers.FindAsync(InputCustomer.Id);
        if (customer == null)
        {
            return NotFound();
        }

        customer.FullName = InputCustomer.FullName;
        customer.Email = InputCustomer.Email;
        customer.Phone = InputCustomer.Phone;
        customer.Address = InputCustomer.Address;

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public class CustomerTotal
    {
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
    }
}
