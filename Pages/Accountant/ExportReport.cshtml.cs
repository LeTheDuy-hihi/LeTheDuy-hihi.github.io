using AutoGarageManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace AutoGarageManager.Pages.Accountant;

[Authorize(Roles = "Accountant,Admin")]
public class ExportReportModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ExportReportModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnGetAsync(string? type)
    {
        type ??= "monthly";
        var now = DateTime.UtcNow;

        if (type.Equals("monthly", StringComparison.OrdinalIgnoreCase))
        {
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var services = await _context!.ServiceHistories
                .Include(s => s.Vehicle)
                    .ThenInclude(v => v!.Customer)
                .Include(s => s.Mechanic)
                .Where(s => s.ServiceDate >= monthStart && s.ServiceDate < monthEnd)
                .OrderBy(s => s.ServiceDate)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Ngày dịch vụ,Khách hàng,Biển số,Thợ,Giá trị (VNĐ)");

            foreach (var svc in services)
            {
                var customerName = svc.Vehicle?.Customer?.FullName ?? string.Empty;
                var plate = svc.Vehicle?.LicensePlate ?? string.Empty;
                var mechanic = svc.Mechanic?.FullName ?? svc.MechanicId;
                csv.AppendLine($"{svc.ServiceDate:yyyy-MM-dd},{EscapeCsv(customerName)},{EscapeCsv(plate)},{EscapeCsv(mechanic)},{svc.TotalCost}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"monthly-report-{now:yyyy-MM}.csv");
        }

        if (type.Equals("yearly", StringComparison.OrdinalIgnoreCase))
        {
            var yearStart = new DateTime(now.Year, 1, 1);
            var yearEnd = yearStart.AddYears(1);

            var services = await _context.ServiceHistories
                .Where(s => s.ServiceDate >= yearStart && s.ServiceDate < yearEnd)
                .ToListAsync();

            var summary = services
                .GroupBy(s => s.ServiceDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(s => s.TotalCost),
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Tháng,Số hóa đơn,Doanh thu (VNĐ)");

            foreach (var item in summary)
            {
                csv.AppendLine($"{item.Month},{item.Count},{item.Total}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"yearly-report-{now:yyyy}.csv");
        }

        if (type.Equals("parts", StringComparison.OrdinalIgnoreCase))
        {
            var parts = await _context.Parts.ToListAsync();
            var csv = new StringBuilder();
            csv.AppendLine("Tên phụ tùng,Loại,Số lượng,Gía (VNĐ)");
            foreach (var p in parts)
            {
                csv.AppendLine($"{EscapeCsv(p.Name)},{EscapeCsv(p.Category)},{p.Quantity},{p.Price}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"parts-report-{now:yyyyMMdd}.csv");
        }

        return NotFound();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }
}
