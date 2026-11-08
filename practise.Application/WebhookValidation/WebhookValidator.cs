using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace practise.Application.WebhookValidation;

public class WebhookValidator(IOptions<BanksSecrets> banksSecrets) : IWebhookValidator
{
    public bool Validate(string payload, string signature,string? bankName)
    {
        if(string.IsNullOrEmpty(bankName))
        {
            throw new ArgumentException("Bank name is required for validation.", nameof(bankName));
        }
        var bankSecrets = banksSecrets.Value;
        var secret = bankSecrets._banksSecrets.GetValueOrDefault(bankName);
        if(string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("Webhook secret is not configured.");
        }

        var expectedSignature = ComputeSignature(secret, payload);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(expectedSignature),
            Convert.FromHexString(signature)
        );
    }

    public static string ComputeSignature(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }

}
