using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AutoGarageManager.Pages;

public class ContactModel : PageModel
{
    public string? Message { get; set; }

    public void OnGet()
    {
    }

    public void OnPost()
    {
        // Get form data
        var name = Request.Form["name"];
        var email = Request.Form["email"];
        var phone = Request.Form["phone"];
        var subject = Request.Form["subject"];
        var message = Request.Form["message"];

        // Set message to display on page
        Message = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
    }
}