using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace wema_Test_Backend_Engineer_Afeez.Services.Implementation
{
    public class GenericService
    {

        public static string Serialize<T>(T obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serialization failed: {ex.Message}");
                return null;
            }
        }
        public static T Deserialize<T>(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization failed: {ex.Message}");
                return default(T);
            }
        }
        public static string GenerateOTP(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentException("Length must be a positive number.");
            }

            // Characters that can be used in the OTP
            const string characters = "0123456789";
            StringBuilder otp = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(characters.Length);
                otp.Append(characters[index]);
            }

            return otp.ToString();
        }
        public static bool VerifyPhoneNumber(string phoneNumber)
        {
            // Check if the phone number is exactly 11 digits
            if (phoneNumber.Length != 11)
            {
                return false;
            }

            // Check if the phone number is numeric
            if (!Regex.IsMatch(phoneNumber, @"^\d{11}$"))
            {
                return false;
            }

            // Check if the phone number starts with a valid Nigerian prefix
            string[] validPrefixes = { "070", "080", "081", "090", "091" };
            foreach (var prefix in validPrefixes)
            {
                if (phoneNumber.StartsWith(prefix))
                {
                    return true;
                }
            }

            return false;
        }
        public static async Task<string> SendRequestAsync(string url, HttpMethod method, string content = null)
        {
            var request = new HttpRequestMessage(method, url);

            if (method != HttpMethod.Get && !string.IsNullOrEmpty(content))
            {
                request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            }

            var response = await new HttpClient().SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return $"Request failed with status code {response.StatusCode}";
            }
        }
    }
}
