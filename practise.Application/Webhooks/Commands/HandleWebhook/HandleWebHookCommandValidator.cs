using FluentValidation;

namespace practise.Application.Webhooks.Commands.HandleWebhook;

public class HandleWebhookCommandValidator : AbstractValidator<HandleWebhookCommand>
{
    public HandleWebhookCommandValidator()
    {
        RuleFor(x => x.DateTime)
            .NotEmpty().WithMessage("DateTime is required.");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("BankName is required.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("Data is required.");
    }
}