namespace practise.Application.WebhookValidation;

public interface IWebhookValidator
{
    bool Validate(string payload, string signature,string? bankName);
}