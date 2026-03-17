using System.Threading.Tasks;

namespace AutoGarageManager.Services
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
    }
}
