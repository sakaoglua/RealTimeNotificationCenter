using System.ComponentModel.DataAnnotations;

namespace RealTimeNotificationCenter.Models.ViewModels
{
    public record SignInViewModel(
        [Required] string UserNameOrEmail,
        [Required] string Password
    );
}
