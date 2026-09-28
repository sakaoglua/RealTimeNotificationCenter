using System.ComponentModel.DataAnnotations;

namespace RealTimeNotificationCenter.Models.ViewModels
{
    public record SignInViewModel([Required] string Email, [Required] string Password);
}
