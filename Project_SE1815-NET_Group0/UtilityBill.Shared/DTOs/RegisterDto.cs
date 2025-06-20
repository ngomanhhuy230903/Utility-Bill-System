// File: UtilityBill.Api/DTOs/RegisterDto.cs
using System.ComponentModel.DataAnnotations;

namespace UtilityBill.Shared.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }
}