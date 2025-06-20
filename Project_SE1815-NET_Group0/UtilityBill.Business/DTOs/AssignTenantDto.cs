// File: UtilityBill.Business/DTOs/AssignTenantDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace UtilityBill.Business.DTOs
{
    public class AssignTenantDto
    {
        [Required(ErrorMessage = "Mã khách thuê là bắt buộc.")]
        public string TenantId { get; set; }

        [Required(ErrorMessage = "Ngày chuyển vào là bắt buộc.")]
        public DateTime MoveInDate { get; set; }
    }
}