using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Hospital.Helpers
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayComparer());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayComparer());

        public void AddRequestData(string key, string value) => _requestData.Add(key, value);
        public void AddResponseData(string key, string value) => _responseData.Add(key, value);
        public string GetResponseData(string key) => _responseData.TryGetValue(key, out var val) ? val : "";

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    // CHUẨN 2.1.0: Chỉ Encode Value, viết HOA mã Hex (%3A thay vì %3a)
                    data.Append(kv.Key + "=" + CustomUrlEncode(kv.Value) + "&");
                }
            }

            string queryString = data.ToString();
            if (queryString.EndsWith("&")) queryString = queryString.Remove(queryString.Length - 1);

            // SỬA: Sử dụng vnp_HashSecret truyền từ Controller vào, KHÔNG hardcode mã cũ
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret.Trim(), queryString);
            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                {
                    data.Append(kv.Key + "=" + CustomUrlEncode(kv.Value) + "&");
                }
            }
            string rawData = data.ToString();
            if (rawData.EndsWith("&")) rawData = rawData.Remove(rawData.Length - 1);

            return HmacSHA512(secretKey.Trim(), rawData).Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        // Hàm ép mã hóa URL sang chữ HOA theo chuẩn VNPAY
        private string CustomUrlEncode(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            string encoded = WebUtility.UrlEncode(input).Replace("+", "%20");
            return Regex.Replace(encoded, "(%[0-9a-f]{2})", m => m.Value.ToUpper());
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue) hash.Append(theByte.ToString("X2")); // Viết HOA mã băm
            }
            return hash.ToString();
        }
    }

    public class VnPayComparer : IComparer<string>
    {
        public int Compare(string x, string y) => string.CompareOrdinal(x, y); // Sắp xếp chuẩn Binary
    }
}