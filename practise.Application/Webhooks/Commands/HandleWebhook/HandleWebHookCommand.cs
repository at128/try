using MediatR;

namespace practise.Application.Webhooks.Commands.HandleWebhook;

public sealed record HandleWebhookCommand(DateTime DateTime, string BankName, string Data) : IRequest<bool>;