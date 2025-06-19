using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBill.Business.Services
{
    public class BillingConfigService : IBillingConfigService
    {
        // Trong thực tế, các giá trị này sẽ được đọc từ file config hoặc database
        private readonly decimal _electricPrice = 3500; // Giá điện: 3,500 VND/kWh
        private readonly decimal _waterPrice = 2400;    // Giá nước: 2,400 VND/m³ (sai trong script, đây là giá ví dụ)

        public decimal GetElectricPrice() => _electricPrice;
        public decimal GetWaterPrice() => _waterPrice;
    }
}
