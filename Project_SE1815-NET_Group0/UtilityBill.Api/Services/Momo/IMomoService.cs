using UtilityBill.Data.Models;
using UtilityBill.Data.Models.Momo;

namespace UtilityBill.Api.Services.Momo;

public interface IMomoService
{
    Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfo model);
    MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
}