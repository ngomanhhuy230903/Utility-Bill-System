using UtilityBill.Api.Libraries;
using UtilityBill.Data.Models.VnPay;

namespace UtilityBill.Api.Services.VnPay;

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _configuration;

    public VnPayService( IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
    {
        try
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            
            // Use the correct configuration key for return URL
            var urlCallBack = _configuration["Vnpay:PaymentBackReturnUrl"];

            Console.WriteLine($"VnPay Configuration:");
            Console.WriteLine($"  BaseUrl: {_configuration["Vnpay:BaseUrl"]}");
            Console.WriteLine($"  TmnCode: {_configuration["Vnpay:TmnCode"]}");
            Console.WriteLine($"  ReturnUrl: {urlCallBack}");
            Console.WriteLine($"  HashSecret: {_configuration["Vnpay:HashSecret"]}");
            Console.WriteLine($"  Model Data:");
            Console.WriteLine($"    Name: {model.Name}");
            Console.WriteLine($"    Amount: {model.Amount}");
            Console.WriteLine($"    OrderDescription: {model.OrderDescription}");
            Console.WriteLine($"    OrderType: {model.OrderType}");

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
            
            Console.WriteLine($"VnPay Payment URL: {paymentUrl}");
            
            // Validate the URL
            if (!paymentUrl.StartsWith("https://sandbox.vnpayment.vn"))
            {
                Console.WriteLine($"ERROR: Generated URL does not start with VnPay sandbox URL!");
                throw new Exception($"Invalid VnPay URL generated: {paymentUrl}");
            }
            
            return paymentUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"VnPay service error: {ex.Message}");
            throw;
        }
    }
    public PaymentResponseModel PaymentExecute(IQueryCollection collections)
    {
        var pay = new VnPayLibrary();
        var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

        return response;
    }

}