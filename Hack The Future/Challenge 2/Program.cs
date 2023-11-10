using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace Challenge2
{
    internal class Program
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        public static async Task Main(string[] args)
        {
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Team", "5cd1d9fe-e26c-4c74-a5c9-9429af9320e2");
            var responseContent = await HttpClient.GetStringAsync("https://exs-htf-2023.azurewebsites.net/api/challenges/ruins?isTest=False");
            WeirdSymbols encryptedSymbols = JsonConvert.DeserializeObject<WeirdSymbols>(responseContent);
            object answer = new { answer = ReplaceRandomSymbols(encryptedSymbols.Symbols) };
            var response = await HttpClient.PutAsJsonAsync("https://exs-htf-2023.azurewebsites.net/api/challenges/ruins", answer);

            var responseContent2 = await response.Content.ReadAsStringAsync();
            dynamic responseObj = JsonConvert.DeserializeObject(responseContent2);
            string encryptedSymbols2 = responseObj.encryption;

            string key = "Pyramids of Giza";
            string iv = "Valley of Kings0";
            string decryptedString = AESDecryptor.Decrypt(encryptedSymbols2, key, iv);

            object answer2 = new { answer = decryptedString };
            var response2 = await HttpClient.PostAsJsonAsync("https://exs-htf-2023.azurewebsites.net/api/challenges/ruins", answer2);
        }

        public static readonly Dictionary<char, char> WeirdSymbolsDict = new Dictionary<char, char>()
        {
        { '₳', 'A' },
        { '฿', 'B' },
        { '₵', 'C' },
        { '₫', 'D' },
        { '€', 'E' },
        { '₣', 'F' },
        { '₲', 'G' },
        { '₶', 'H' },
        { '₻', 'I' },
        { '৳', 'J' },
        { '₭', 'K' },
        { '£', 'L' },
        { 'ℳ', 'M' },
        { '₦', 'N' },
        { '¤', 'O' },
        { '₱', 'P' },
        { '֏', 'Q' },
        { '₨', 'R' },
        { '$', 'S' },
        { '₸', 'T' },
        { '₼', 'U' },
        { '₹', 'V' },
        { '₩', 'W' },
        { '₪', 'X' },
        { '¥', 'Y' },
        { '₷', 'Z' }
        };

        public static string ReplaceRandomSymbols(string text)
        {
            var symbols = new List<char>(text.Distinct());
            var replacements = new Dictionary<char, char>();
            foreach (var symbol in symbols)
            {
                if (WeirdSymbolsDict.ContainsKey(symbol))
                {
                    replacements[symbol] = WeirdSymbolsDict[symbol];
                }
            }

            var sb = new StringBuilder(text);
            foreach (var symbol in replacements.Keys)
            {
                sb.Replace(symbol, replacements[symbol]);
            }

            return sb.ToString();
        }

        public class WeirdSymbols
        {
            public string Symbols { get; set; }
        }

        public class AESDecryptor
        {
            public static string Decrypt(string encryptedText, string key, string iv)
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = ivBytes;
                    aes.Mode = CipherMode.CBC;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (var ms = new System.IO.MemoryStream(encryptedBytes))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new System.IO.StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}