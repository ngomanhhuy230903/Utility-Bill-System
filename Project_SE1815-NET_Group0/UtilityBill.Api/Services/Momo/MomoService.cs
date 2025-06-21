using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using UtilityBill.Data.Models;
using UtilityBill.Data.Models.Momo;

namespace UtilityBill.Api.Services.Momo;

public class MomoService : IMomoService
{
    private readonly IOptions<MomoOptionModel> _options;

    public MomoService(IOptions<MomoOptionModel> options)
    {
        _options = options;
    }

    public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfo model)
    {
        try
        {
            model.OrderId = DateTime.UtcNow.Ticks.ToString();
            model.OrderInformation = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInformation;
            var rawData =
                $"partnerCode={_options.Value.PartnerCode}" +
                $"&accessKey={_options.Value.AccessKey}" +
                $"&requestId={model.OrderId}" +
                $"&amount={model.Amount}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={model.OrderInformation}" +
                $"&returnUrl={_options.Value.ReturnUrl}" +
                $"&notifyUrl={_options.Value.NotifyUrl}" +
                $"&extraData=";

            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");
            
            // Create an object representing the request data
            var requestData = new
            {
                accessKey = _options.Value.AccessKey,
                partnerCode = _options.Value.PartnerCode,
                requestType = _options.Value.RequestType,
                notifyUrl = _options.Value.NotifyUrl,
                returnUrl = _options.Value.ReturnUrl,
                orderId = model.OrderId,
                amount = model.Amount.ToString(),
                orderInfo = model.OrderInformation,
                requestId = model.OrderId,
                extraData = "",
                signature = signature
            };

            var requestJson = JsonConvert.SerializeObject(requestData);
            request.AddParameter("application/json", requestJson, ParameterType.RequestBody);

            Console.WriteLine($"MoMo API Request: {requestJson}");
            Console.WriteLine($"MoMo API URL: {_options.Value.MomoApiUrl}");

            var response = await client.ExecuteAsync(request);
            
            Console.WriteLine($"MoMo API Response Status: {response.StatusCode}");
            Console.WriteLine($"MoMo API Response Content: {response.Content}");

            if (!response.IsSuccessful)
            {
                Console.WriteLine($"MoMo API call failed: {response.ErrorMessage}");
                return new MomoCreatePaymentResponseModel
                {
                    ErrorCode = -1,
                    Message = $"API call failed: {response.ErrorMessage}",
                    LocalMessage = "Không thể kết nối đến MoMo API"
                };
            }

            var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
            
            Console.WriteLine($"MoMo Response PayUrl: {momoResponse?.PayUrl}");
            Console.WriteLine($"MoMo Response ErrorCode: {momoResponse?.ErrorCode}");
            
            return momoResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MoMo service error: {ex.Message}");
            return new MomoCreatePaymentResponseModel
            {
                ErrorCode = -1,
                Message = $"Service error: {ex.Message}",
                LocalMessage = "Lỗi dịch vụ MoMo"
            };
        }
    }

    public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
    {
        var amount = collection.First(s => s.Key == "amount").Value;
        var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
        var orderId = collection.First(s => s.Key == "orderId").Value;

        return new MomoExecuteResponseModel()
        {
            Amount = amount,
            OrderId = orderId,
            OrderInfo = orderInfo

        };
    }

    private string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        byte[] hashBytes;

        using (var hmac = new HMACSHA256(keyBytes))
        {
            hashBytes = hmac.ComputeHash(messageBytes);
        }

        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return hashString;
    }
}

