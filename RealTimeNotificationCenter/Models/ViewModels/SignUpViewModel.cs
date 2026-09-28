using System.ComponentModel.DataAnnotations;

namespace RealTimeNotificationCenter.Models.ViewModels
{
    public record SignUpViewModel(
        [Required] string UserName,
        [Required] string Email,
        [Required] string Password,
        [Required] string ConfirmPassword
    );
}
