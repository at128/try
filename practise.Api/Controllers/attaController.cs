using MediatR;
using Microsoft.AspNetCore.Mvc;
using practise.Application.Webhooks.Commands.HandleWebhook;
using practise.Application.WebhookValidation;
using practise.Domain.Parsers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace practise.Api.Controllers;


[ApiController]
public class ReceiveMoneyController: ControllerBase
{
    private IWebhookValidator _webhookValidator;
    private IMediator _mediator;

    public ReceiveMoneyController(IWebhookValidator webhookValidator, IMediator mediator)
    {
        _webhookValidator = webhookValidator;
        _mediator = mediator;
    }

    [HttpPost("receive-money")]
    public async Task<IActionResult> ReceiveMoney()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["X-Signature"].ToString();

        var webhookData = JsonSerializer.Deserialize<WebhookData>(payload);
        
        if (webhookData is null)
        {
            return BadRequest();
        }

        if(string.IsNullOrEmpty(webhookData._bankName))
        {
            return BadRequest();
        }

        if(webhookData._timestamp < DateTime.UtcNow.AddMinutes(-5))
        {
            return BadRequest();
        }

        
        if (!_webhookValidator.Validate(payload, signature,webhookData._bankName))
        {
            return Unauthorized();
        }
        
        

        var result = await _mediator.Send(new HandleWebhookCommand(webhookData._timestamp, webhookData._bankName, webhookData._data));
        
        if (!result)
        {
            return BadRequest();
        }
        
        return Ok();
    }

    


    
}

public class WebhookData
{
    public DateTime _timestamp { get; set; } = DateTime.UtcNow;
    public string _bankName { get; set; }= string.Empty;
    public string _data { get; set; } = string.Empty;
}