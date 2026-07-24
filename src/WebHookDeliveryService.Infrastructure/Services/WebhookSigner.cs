using System.Security.Cryptography;
using System.Text;
using WebHookDeliveryService.Domain.Interfaces;

namespace WebHookDeliveryService.Infrastructure.Services;

public class WebhookSigner : IWebhookSigner
{
    public string Sign(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool Verify(string secret, string payload, string receivedSignature)
    {
        var expected = Sign(secret, payload);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(receivedSignature));
    }

    public string ComputeSignature(string secret, long timestamp, string body)
    {
        var payload = $"{timestamp}.{body}";
        return Sign(secret, payload);
    }

    public bool VerifyWithTimestamp(string secret, string body, string receivedSignature, long toleranceSeconds = 300)
    {
        var parts = receivedSignature.Split(',');
        if (parts.Length != 2) return false;

        var tsPart = parts[0];
        var sigPart = parts[1];

        if (!tsPart.StartsWith("t=") || !sigPart.StartsWith("v1=")) return false;

        if (!long.TryParse(tsPart[2..], out var timestamp)) return false;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - timestamp) > toleranceSeconds) return false;

        var expected = ComputeSignature(secret, timestamp, body);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(sigPart[3..]));
    }
}
