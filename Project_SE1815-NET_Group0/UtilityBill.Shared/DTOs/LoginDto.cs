// File: UtilityBill.Api/DTOs/LoginDto.cs
using System.ComponentModel.DataAnnotations;

namespace UtilityBill.Shared.DTOs
{
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}