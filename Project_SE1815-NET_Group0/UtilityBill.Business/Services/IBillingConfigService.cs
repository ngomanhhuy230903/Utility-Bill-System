using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBill.Business.Services
{
    public interface IBillingConfigService
    {
        decimal GetElectricPrice();
        decimal GetWaterPrice();
    }
}
