using System.Text;
using System.Text.Json;
using System.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using practise.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace practise.Application.BackgroundJobs;

public class TransactionsHandlerBackgroundService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionsHandlerBackgroundService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;


    public TransactionsHandlerBackgroundService(
        IConfiguration configuration,
        IServiceProvider serviceProvider, 
        ILogger<TransactionsHandlerBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _channel = await CreateRabbitMqChannelAsync();
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                
                using var scope = _serviceProvider.CreateScope();
                var clientRepository = scope.ServiceProvider.GetRequiredService<IClientRepository>();
                var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                try
                {
                    // i should wrape all this in a transaction to do atomic changes

                    using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var transaction = JsonSerializer.Deserialize<Domain.Models.Transaction>(message);
                        
                        if (transaction != null)
                        {
                            

                            var isTransactionProcessed = await transactionRepository.IsTransactionProcessedAsync(transaction.Id, transaction.Date);

                            
                            if (isTransactionProcessed)
                            {
                                _logger.LogWarning($"The transaction is already processed: {transaction.Id}");
                                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                                return ;
                            }

                            var result= await clientRepository.AddBalanceAsync(transaction.TrasferTo, transaction.Amount);
                            
                            if (!result)
                            {
                                throw new InvalidOperationException($"Client not found: {transaction.TrasferTo}");
                            }

                            var isTransactionMarkedAsProcessed = await transactionRepository.MarkTransactionAsProcessedAsync(transaction.Id, transaction.Date);
                            
                            if (!isTransactionMarkedAsProcessed)
                            {
                                _logger.LogWarning($"Failed to mark transaction as processed: {transaction.Id}");
                                throw new Exception("Error when marking the transaction as processed");
                            }

                            _logger.LogInformation($"Updated balance for account: {transaction.TrasferTo}");
                            
                            
                        }
                        transactionScope.Complete();
                        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    }

                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing transaction message: {Message}", message);
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await _channel.BasicConsumeAsync("transaction_queue", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            var status = "Active";
            while (!stoppingToken.IsCancellationRequested)
            {
                var isTransactionHandlerActivated = _configuration.GetValue<bool>("IsTransactionHandlerActivated");
                if (isTransactionHandlerActivated)
                {
                    if(status!="Active")
                    {
                        
                        await _channel.BasicConsumeAsync("transaction_queue", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
                        _logger.LogInformation("TransactionsHandlerBackgroundService is now active.");
                        status = "Active";
                    }
                }
                else
                {
                    if(status!="Inactive")
                    {
                        await _channel.BasicCancelAsync(consumer.ConsumerTags[0], cancellationToken: stoppingToken);
                        _logger.LogInformation("TransactionsHandlerBackgroundService is now inactive.");
                        status = "Inactive";
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("TransactionsHandlerBackgroundService is stopping due to cancellation.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in TransactionsHandlerBackgroundService.");
        }
    }

    private async Task<IChannel> CreateRabbitMqChannelAsync()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        
        _connection = await factory.CreateConnectionAsync();
        var channel = await _connection.CreateChannelAsync();
        
        await channel.QueueDeclareAsync(queue: "transaction_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        
        return channel;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync(cancellationToken);
        if (_connection != null) await _connection.CloseAsync(cancellationToken);
        
        await base.StopAsync(cancellationToken);
    }
}