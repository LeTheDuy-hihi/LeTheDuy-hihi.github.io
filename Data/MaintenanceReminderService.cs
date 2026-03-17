using AutoGarageManager.Models;
using AutoGarageManager.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Data
{
    public class MaintenanceReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MaintenanceReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Check daily

        public MaintenanceReminderService(IServiceProvider serviceProvider, ILogger<MaintenanceReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Maintenance Reminder Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndCreateRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking maintenance reminders.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Maintenance Reminder Service is stopping.");
        }

        private async Task CheckAndCreateRemindersAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();

                // Get all vehicles
                var vehicles = await context.Vehicles.ToListAsync();

                foreach (var vehicle in vehicles)
                {
                    // 6-month service reminder based on last service date
                    var lastService = await context.ServiceHistories
                        .Where(sh => sh.VehicleId == vehicle.Id)
                        .OrderByDescending(sh => sh.ServiceDate)
                        .FirstOrDefaultAsync();

                    if (lastService != null && lastService.ServiceDate.AddMonths(6) <= DateTime.Now)
                    {
                        var existingSixMonthReminder = await context.MaintenanceReminders
                            .FirstOrDefaultAsync(r => r.VehicleId == vehicle.Id &&
                                                     r.Title == "6-Month Maintenance" &&
                                                     !r.IsCompleted);

                        if (existingSixMonthReminder == null)
                        {
                            var reminder = new MaintenanceReminder
                            {
                                VehicleId = vehicle.Id,
                                Title = "6-Month Maintenance",
                                Message = $"Maintenance reminder: {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate}) last serviced on {lastService.ServiceDate:dd/MM/yyyy}.",
                                ReminderDate = DateTime.Now
                            };

                            context.MaintenanceReminders.Add(reminder);
                            _logger.LogInformation($"Created 6-month maintenance reminder for vehicle {vehicle.Id}");

                            // Send SMS if phone number exists
                            var vehicleWithCustomer = await context.Vehicles
                                .Include(v => v.Customer)
                                .FirstOrDefaultAsync(v => v.Id == vehicle.Id);

                            if (vehicleWithCustomer?.Customer != null && !string.IsNullOrEmpty(vehicleWithCustomer.Customer.Phone))
                            {
                                var smsMessage = $"Nhắc nhở: Xe {vehicleWithCustomer.Make} {vehicleWithCustomer.Model} ({vehicleWithCustomer.LicensePlate}) cần bảo dưỡng định kỳ 6 tháng. Liên hệ gara để được hỗ trợ.";
                                var smsSent = await smsService.SendSmsAsync(vehicleWithCustomer.Customer.Phone, smsMessage);
                                if (smsSent)
                                {
                                    reminder.IsSent = true;
                                    _logger.LogInformation($"SMS sent to {vehicleWithCustomer.Customer.Phone} for 6-month maintenance.");
                                }
                            }
                        }
                    }

                    // Check for oil change (every 5000 miles)
                    if (vehicle.Mileage > 0 && vehicle.Mileage % 5000 == 0)
                    {
                        var existingReminder = await context.MaintenanceReminders
                            .FirstOrDefaultAsync(r => r.VehicleId == vehicle.Id &&
                                                     r.Title == "Oil Change" &&
                                                     !r.IsCompleted &&
                                                     r.ReminderDate > DateTime.Now);

                        if (existingReminder == null)
                        {
                            var reminder = new MaintenanceReminder
                            {
                                VehicleId = vehicle.Id,
                                Title = "Oil Change",
                                Message = $"Oil change due for {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate}) at {vehicle.Mileage} miles",
                                ReminderDate = DateTime.Now.AddDays(7) // Due in 7 days
                            };

                            context.MaintenanceReminders.Add(reminder);
                            _logger.LogInformation($"Created oil change reminder for vehicle {vehicle.Id}");

                            // Send SMS if phone number exists
                            var customerInfo = await context.Vehicles
                                .Include(v => v.Customer)
                                .FirstOrDefaultAsync(v => v.Id == vehicle.Id);

                            if (customerInfo?.Customer != null && !string.IsNullOrEmpty(customerInfo.Customer.Phone))
                            {
                                var smsMessage = $"Nhắc nhở: Xe {customerInfo.Make} {customerInfo.Model} ({customerInfo.LicensePlate}) cần thay dầu động cơ. Liên hệ gara để đặt lịch sửa chữa.";
                                var smsSent = await smsService.SendSmsAsync(customerInfo.Customer.Phone, smsMessage);
                                if (smsSent)
                                {
                                    reminder.IsSent = true;
                                    _logger.LogInformation($"SMS sent to {customerInfo.Customer.Phone} for oil change reminder.");
                                }
                            }
                        }
                    }

                    // Check for tire rotation (every 8000 miles)
                    if (vehicle.Mileage > 0 && vehicle.Mileage % 8000 == 0)
                    {
                        var existingReminder = await context.MaintenanceReminders
                            .FirstOrDefaultAsync(r => r.VehicleId == vehicle.Id &&
                                                     r.Title == "Tire Rotation" &&
                                                     !r.IsCompleted &&
                                                     r.ReminderDate > DateTime.Now);

                        if (existingReminder == null)
                        {
                            var reminder = new MaintenanceReminder
                            {
                                VehicleId = vehicle.Id,
                                Title = "Tire Rotation",
                                Message = $"Tire rotation due for {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate}) at {vehicle.Mileage} miles",
                                ReminderDate = DateTime.Now.AddDays(7)
                            };

                            context.MaintenanceReminders.Add(reminder);
                            _logger.LogInformation($"Created tire rotation reminder for vehicle {vehicle.Id}");

                            // Send SMS if phone number exists
                            var customerInfo = await context.Vehicles
                                .Include(v => v.Customer)
                                .FirstOrDefaultAsync(v => v.Id == vehicle.Id);

                            if (customerInfo?.Customer != null && !string.IsNullOrEmpty(customerInfo.Customer.Phone))
                            {
                                var smsMessage = $"Nhắc nhở: Xe {customerInfo.Make} {customerInfo.Model} ({customerInfo.LicensePlate}) cần xoay vòng bánh xe. Liên hệ gara để đặt lịch sửa chữa.";
                                var smsSent = await smsService.SendSmsAsync(customerInfo.Customer.Phone, smsMessage);
                                if (smsSent)
                                {
                                    reminder.IsSent = true;
                                    _logger.LogInformation($"SMS sent to {customerInfo.Customer.Phone} for tire rotation reminder.");
                                }
                            }
                        }
                    }

                    // Check for brake inspection (every 12000 miles)
                    if (vehicle.Mileage > 0 && vehicle.Mileage % 12000 == 0)
                    {
                        var existingReminder = await context.MaintenanceReminders
                            .FirstOrDefaultAsync(r => r.VehicleId == vehicle.Id &&
                                                     r.Title == "Brake Inspection" &&
                                                     !r.IsCompleted &&
                                                     r.ReminderDate > DateTime.Now);

                        if (existingReminder == null)
                        {
                            var reminder = new MaintenanceReminder
                            {
                                VehicleId = vehicle.Id,
                                Title = "Brake Inspection",
                                Message = $"Brake inspection due for {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate}) at {vehicle.Mileage} miles",
                                ReminderDate = DateTime.Now.AddDays(7)
                            };

                            context.MaintenanceReminders.Add(reminder);
                            _logger.LogInformation($"Created brake inspection reminder for vehicle {vehicle.Id}");

                            // Send SMS if phone number exists
                            var customerInfo = await context.Vehicles
                                .Include(v => v.Customer)
                                .FirstOrDefaultAsync(v => v.Id == vehicle.Id);

                            if (customerInfo?.Customer != null && !string.IsNullOrEmpty(customerInfo.Customer.Phone))
                            {
                                var smsMessage = $"Nhắc nhở: Xe {customerInfo.Make} {customerInfo.Model} ({customerInfo.LicensePlate}) cần kiểm tra hệ thống phanh. Liên hệ gara để đặt lịch sửa chữa.";
                                var smsSent = await smsService.SendSmsAsync(customerInfo.Customer.Phone, smsMessage);
                                if (smsSent)
                                {
                                    reminder.IsSent = true;
                                    _logger.LogInformation($"SMS sent to {customerInfo.Customer.Phone} for brake inspection reminder.");
                                }
                            }
                        }
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}