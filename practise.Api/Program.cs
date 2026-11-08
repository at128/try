using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using practise.Application;
using practise.Application.BackgroundJobs;
using practise.Application.Behaviors;
using practise.Application.Interfaces;
using practise.Application.WebhookValidation;
using practise.Domain.Parsers;
using practise.Infrastructure.Data;
using practise.Infrastructure.Repsitories;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(options =>
{
   options.RegisterServicesFromAssembly(typeof(IAssemblyMarker).Assembly); 
});

builder.Services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddControllers().AddNewtonsoftJson();;

builder.Services.AddScoped<ITransactionRepository,TransactionRepository>();
builder.Services.AddSingleton<ShardResolver>();
builder.Services.AddSingleton<IWebhookValidator,WebhookValidator>();

builder.Services.AddSingleton<ITransactionParser, AcmeBankParser>();
builder.Services.AddSingleton<ITransactionParser, PayTechBankParser>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();

builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("baseConnectionString")));

builder.Services.AddHostedService<TransactionsHandlerBackgroundService>();

// builder.Configuration.AddAzureKeyVault(
//     new Uri(builder.Configuration["keyVault:VaultUri"]!),
//     new DefaultAzureCredential()
// );

builder.Services.Configure<BanksSecrets>(builder.Configuration.GetSection("BanksSecrets"));

var app = builder.Build();

app.MapControllers();

app.Run();


