using WebHookDeliveryService.Infrastructure.Services;

namespace WebHookDeliveryService.Tests;

public class WebhookSignerTests
{
    private readonly WebhookSigner _signer = new();

    [Fact]
    public void Sign_ReturnsConsistentHmac()
    {
        var secret = "test-secret-key";
        var payload = "hello world";

        var sig1 = _signer.Sign(secret, payload);
        var sig2 = _signer.Sign(secret, payload);

        Assert.Equal(sig1, sig2);
        Assert.Equal(64, sig1.Length); // SHA256 hex = 64 chars
    }

    [Fact]
    public void Sign_DifferentSecrets_ProduceDifferentSignatures()
    {
        var payload = "hello world";

        var sig1 = _signer.Sign("secret-1", payload);
        var sig2 = _signer.Sign("secret-2", payload);

        Assert.NotEqual(sig1, sig2);
    }

    [Fact]
    public void Verify_ValidSignature_ReturnsTrue()
    {
        var secret = "test-secret";
        var payload = "test-payload";

        var signature = _signer.Sign(secret, payload);
        var result = _signer.Verify(secret, payload, signature);

        Assert.True(result);
    }

    [Fact]
    public void Verify_InvalidSignature_ReturnsFalse()
    {
        var secret = "test-secret";
        var payload = "test-payload";

        var result = _signer.Verify(secret, payload, "invalid-signature");

        Assert.False(result);
    }

    [Fact]
    public void VerifyWithTimestamp_ValidSignature_ReturnsTrue()
    {
        var secret = "test-secret";
        var body = "{\"event\":\"test\"}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var signature = _signer.ComputeSignature(secret, timestamp, body);
        var receivedSignature = $"t={timestamp},v1={signature}";

        var result = _signer.VerifyWithTimestamp(secret, body, receivedSignature);

        Assert.True(result);
    }

    [Fact]
    public void VerifyWithTimestamp_ExpiredTimestamp_ReturnsFalse()
    {
        var secret = "test-secret";
        var body = "{\"event\":\"test\"}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 600; // 10 minutes ago

        var signature = _signer.ComputeSignature(secret, timestamp, body);
        var receivedSignature = $"t={timestamp},v1={signature}";

        var result = _signer.VerifyWithTimestamp(secret, body, receivedSignature, toleranceSeconds: 300);

        Assert.False(result);
    }

    [Fact]
    public void VerifyWithTimestamp_WrongBody_ReturnsFalse()
    {
        var secret = "test-secret";
        var body = "{\"event\":\"test\"}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var signature = _signer.ComputeSignature(secret, timestamp, body);
        var receivedSignature = $"t={timestamp},v1={signature}";

        var result = _signer.VerifyWithTimestamp(secret, "{\"event\":\"tampered\"}", receivedSignature);

        Assert.False(result);
    }

    [Fact]
    public void VerifyWithTimestamp_MalformedSignature_ReturnsFalse()
    {
        var result = _signer.VerifyWithTimestamp("secret", "body", "bad-format");

        Assert.False(result);
    }
}
