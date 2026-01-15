using System.Net;
using System.Security.Cryptography;
using System.Text;

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
            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    // Dùng WebUtility.UrlEncode và sửa dấu + thành %20 cho đúng chuẩn VNPAY
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value).Replace("+", "%20") + "&");
                }
            }

            string queryString = data.ToString();
            if (queryString.EndsWith("&"))
            {
                queryString = queryString.Remove(queryString.Length - 1);
            }

            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, queryString);
            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            StringBuilder data = new StringBuilder();
            foreach (var kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                {
                    // Tương tự, khi validate cũng phải encode lại các giá trị nhận về
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value).Replace("+", "%20") + "&");
                }
            }
            string rawData = data.ToString();
            if (rawData.EndsWith("&"))
            {
                rawData = rawData.Remove(rawData.Length - 1);
            }

            string myChecksum = HmacSHA512(secretKey, rawData);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString().ToUpper();
        }
    }

    public class VnPayComparer : IComparer<string>
    {
        public int Compare(string x, string y) => string.Compare(x, y, StringComparison.Ordinal);
    }
}