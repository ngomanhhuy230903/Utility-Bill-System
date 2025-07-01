using UtilityBill.Data.Models;
using UtilityBill.Data.Models.VnPay;

namespace UtilityBill.Api.DTOs;

public class UnifiedPaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string OrderDescription { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OrderType { get; set; } = "other";
    
    public PaymentInformationModel ToPaymentInformationModel()
    {
        return new PaymentInformationModel
        {
            OrderType = this.OrderType,
            Amount = this.Amount,
            OrderDescription = this.OrderDescription,
            Name = this.Name
        };
    }

    public OrderInfo ToOrderInfo()
    {
        return new OrderInfo
        {
            FullName = this.Name,
            OrderId = this.OrderId,
            OrderInformation = $"{this.OrderType} \n {this.OrderDescription}",
            Amount = this.Amount
        };
    }
} 