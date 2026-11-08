using System.Text;
using System.Text.Json;
using System.Transactions;
using MediatR;
using Microsoft.Extensions.Logging;
using practise.Application.Interfaces;
using practise.Domain.Models;
using practise.Domain.Parsers;
using RabbitMQ.Client;
namespace practise.Application.Webhooks.Commands.HandleWebhook;

public class HandleWebhookCommandHandler : IRequestHandler<HandleWebhookCommand, bool>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IEnumerable<ITransactionParser> _transactionParsers;
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<HandleWebhookCommandHandler> _logger;

    public HandleWebhookCommandHandler(ITransactionRepository transactionRepository,IEnumerable<ITransactionParser> transactionParsers,IClientRepository clientRepository ,ILogger<HandleWebhookCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _transactionParsers = transactionParsers;
        _clientRepository = clientRepository;
        _logger = logger;
    }



    public async Task<bool> Handle(HandleWebhookCommand request, CancellationToken cancellationToken)
    {

        /*devide the string  into List<string> by new line*/
        var lines = request.Data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var parser = _transactionParsers.FirstOrDefault(p => p.BankName == request.BankName);
        if (parser == null)
        {
            throw  new Exception($"No parser found for bank: {request.BankName}");
        }
        var channel = await CreateRabbitMqChannelAsync();
        

        foreach (var line in lines)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                
                    var transaction = parser.Parse(line);
                    if (transaction == null)
                    {
                        throw new Exception($"Failed to parse transaction for bank: {request.BankName}");
                    }

                    await _transactionRepository.AddTransactionAsync(transaction);    

                    var serializedTransaction = JsonSerializer.Serialize(transaction);
                    var body = Encoding.UTF8.GetBytes(serializedTransaction);

                    await channel.BasicPublishAsync(exchange: string.Empty,
                        routingKey: "transaction_queue",
                        mandatory: true,
                        basicProperties: new BasicProperties { Persistent = true },
                        body: body);

                    _logger.LogInformation($"Updated balance for account: {transaction.TrasferTo}");
                    transactionScope.Complete();
                }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing line: {line} for bank: {request.BankName}");
            }
        }

        return true;
    }

    public async Task<IChannel> CreateRabbitMqChannelAsync()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = await factory.CreateConnectionAsync();
        var channelOpts = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true
        );
        var channel = await connection.CreateChannelAsync(channelOpts);
        await channel.QueueDeclareAsync(queue: "transaction_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        return channel;
    }
}
