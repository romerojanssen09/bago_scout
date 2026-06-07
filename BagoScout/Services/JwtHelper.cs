using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BagoScout.Services
{
    public static class JwtHelper
    {
        // Simple fixed key for local development. In production, this should come from configuration.
        private static readonly string SecretKey = "BagoScoutSuperSecretSecurityKey1234567890!@#";

        public static string GenerateToken(string userId, string email, string userType, string userName, int expiryDays = 30)
        {
            var header = new { alg = "HS256", typ = "JWT" };
            var headerJson = JsonSerializer.Serialize(header);
            var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));

            var expiryUnix = DateTimeOffset.UtcNow.AddDays(expiryDays).ToUnixTimeSeconds();
            var payload = new Dictionary<string, object>
            {
                { "sub", userId },
                { "email", email },
                { "userType", userType },
                { "name", userName },
                { "exp", expiryUnix }
            };
            var payloadJson = JsonSerializer.Serialize(payload);
            var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

            var input = $"{headerBase64}.{payloadBase64}";
            var keyBytes = Encoding.UTF8.GetBytes(SecretKey);
            using var hmac = new HMACSHA256(keyBytes);
            var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            var signatureBase64 = Base64UrlEncode(signatureBytes);

            return $"{input}.{signatureBase64}";
        }

        public static bool ValidateToken(string token, out Dictionary<string, string>? claims)
        {
            claims = null;
            if (string.IsNullOrEmpty(token)) return false;

            var parts = token.Split('.');
            if (parts.Length != 3) return false;

            var headerBase64 = parts[0];
            var payloadBase64 = parts[1];
            var signatureBase64 = parts[2];

            // Verify signature
            var input = $"{headerBase64}.{payloadBase64}";
            var keyBytes = Encoding.UTF8.GetBytes(SecretKey);
            using var hmac = new HMACSHA256(keyBytes);
            var expectedSignatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            var expectedSignatureBase64 = Base64UrlEncode(expectedSignatureBytes);

            if (!string.Equals(signatureBase64, expectedSignatureBase64))
            {
                return false;
            }

            // Decode payload
            try
            {
                var payloadBytes = Base64UrlDecode(payloadBase64);
                var payloadJson = Encoding.UTF8.GetString(payloadBytes);
                var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);
                if (payload == null) return false;

                // Check expiry
                if (payload.TryGetValue("exp", out var expObj))
                {
                    long exp = 0;
                    if (expObj is JsonElement element && element.ValueKind == JsonValueKind.Number)
                    {
                        exp = element.GetInt64();
                    }
                    else if (expObj is long l)
                    {
                        exp = l;
                    }

                    var currentUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (currentUnix > exp)
                    {
                        return false; // Expired
                    }
                }

                // Extract claims
                claims = new Dictionary<string, string>();
                foreach (var kvp in payload)
                {
                    if (kvp.Value is JsonElement jsonEl)
                    {
                        claims[kvp.Key] = jsonEl.ToString();
                    }
                    else
                    {
                        claims[kvp.Key] = kvp.Value?.ToString() ?? "";
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);
            return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var base64 = input.Replace("-", "+").Replace("_", "/");
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
