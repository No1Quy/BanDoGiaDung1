using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class MomoService
{
    private string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
    private string partnerCode = "MOMO";
    private string accessKey = "F8BBA842ECF85";
    private string secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";

    public async Task<string> CreatePaymentAsync(long amount, string orderId, string redirectUrl)
    {
        string orderInfo = "Thanh toán đơn hàng";
        string requestId = Guid.NewGuid().ToString();
        string extraData = "";
        string ipnUrl = redirectUrl; // test thì để cùng redirectUrl

        // 1️⃣ RAW SIGNATURE CHUẨN MOMO V2
        string rawHash =
            $"accessKey={accessKey}" +
            $"&amount={amount}" +
            $"&extraData={extraData}" +
            $"&ipnUrl={ipnUrl}" +
            $"&orderId={orderId}" +
            $"&orderInfo={orderInfo}" +
            $"&partnerCode={partnerCode}" +
            $"&redirectUrl={redirectUrl}" +
            $"&requestId={requestId}" +
            $"&requestType=captureWallet";

        // 2️⃣ TÍNH HMAC SHA256
        string signature = HmacSHA256(rawHash, secretKey);

        // 3️⃣ BODY GỬI LÊN MOMO
        var data = new
        {
            partnerCode = partnerCode,
            requestId = requestId,
            amount = amount,
            orderId = orderId,
            orderInfo = orderInfo,
            redirectUrl = redirectUrl,
            ipnUrl = ipnUrl,
            extraData = extraData,
            requestType = "captureWallet",
            signature = signature,
            lang = "vi"
        };

        string json = JsonConvert.SerializeObject(data);

        using (var client = new HttpClient())
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);

            var result = await response.Content.ReadAsStringAsync();

            dynamic resultJson = JsonConvert.DeserializeObject(result);

            // Trả về URL thanh toán
            return resultJson.payUrl;
        }
    }


    // HMAC SHA256
    private string HmacSHA256(string message, string key)
    {
        var encoding = Encoding.UTF8;
        byte[] keyByte = encoding.GetBytes(key);
        byte[] messageBytes = encoding.GetBytes(message);

        using (var hmacsha256 = new HMACSHA256(keyByte))
        {
            byte[] hashMessage = hmacsha256.ComputeHash(messageBytes);
            return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
        }
    }
}
