using UtilityBill.Data.Models.VnPay;

namespace UtilityBill.Api.Services.VnPay;

public interface IVnPayService
{
    
    string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
    PaymentResponseModel PaymentExecute(IQueryCollection collections);

}