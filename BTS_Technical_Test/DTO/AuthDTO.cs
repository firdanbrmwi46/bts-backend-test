using System.ComponentModel.DataAnnotations;

namespace BTS_Technical_Test.DTO
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Password dan konfirmasi harus sama")]
        public string PasswordConfirmation { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Authentication_token { get; set; } = string.Empty;
        public string Refresh_token { get; set; } = string.Empty;
    }
}