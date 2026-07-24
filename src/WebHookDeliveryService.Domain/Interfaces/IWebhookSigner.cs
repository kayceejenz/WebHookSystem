namespace WebHookDeliveryService.Domain.Interfaces;

public interface IWebhookSigner
{
    string Sign(string secret, string payload);
    bool Verify(string secret, string payload, string receivedSignature);
    string ComputeSignature(string secret, long timestamp, string body);
    bool VerifyWithTimestamp(string secret, string body, string receivedSignature, long toleranceSeconds = 300);
}
