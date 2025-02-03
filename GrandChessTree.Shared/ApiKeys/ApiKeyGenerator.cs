using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GrandChessTree.Shared.ApiKeys
{
    public static class ApiKeyGenerator
    {
        public const int KeyLength = 32;

        private static string GenerateApiKey()
        {
            var bytes = RandomNumberGenerator.GetBytes(KeyLength);

            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "-");
        }

        public static string HashApiKey(string input)
        {
            using var sha256 = SHA256.Create();

            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            var stringBuilder = new StringBuilder();
            foreach (var b in hashBytes)
            {
                stringBuilder.Append(b.ToString("x2"));
            }

            return stringBuilder.ToString();
        }

        public static (string id, string apiKey, string apiKeyTail) Create()
        {
            var apiKey = GenerateApiKey();
            var hashedApiKey = HashApiKey(apiKey);
            return (hashedApiKey, apiKey, apiKey[^3..]);
        }
    }
}
