using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.DTOs;
using System.Text.Json;
using Automatonymous.Binders;

namespace Presentation.API.Controllers
{
    [Route("")]
    [ApiController]
    public class ProxyController : ControllerBase
    {

        private readonly ITransactionService _transactionService;

        private readonly IConfiguration _configuration;

        public ProxyController(IConfiguration configuration, ITransactionService transactionService)
        {
            _transactionService = transactionService;
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDTO transactionDto)
        {
            var transactionId = await _transactionService.CreateTransactionAsync(transactionDto);
            return Ok(new { Key = transactionId });
        }


        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] JsonElement webhookData)
        {

            try
            {
                if (webhookData.TryGetProperty("key", out JsonElement keyElement) && keyElement.TryGetGuid(out Guid transactionId))
                {
                    await _transactionService.HandleWebhookAsync(transactionId, webhookData.ToString());
                    Ok(webhookData);
                }
            }
            catch (Exception ex)
            {

            }

            return Ok(webhookData );

        }

        /*[HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDTO transactionDto)
        {
            if (!ModelState.IsValid || transactionDto.Type == null || !transactionDto.Type.Contains("PROPERTY_INFO"))
            {
                return BadRequest("Invalid request or unsupported dataset.");
            }

            // Create a new scope for the AddTransactionAsync operation
            using var scope = _serviceScopeFactory.CreateScope();
            var propertyService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
            var transactionId = await propertyService.AddTransactionAsync(transactionDto);

            // Process the transaction asynchronously without awaiting
           // ProcessTransactionAsync(transactionId).ConfigureAwait(false);

            // Return transaction ID to the client immediately
            return Ok(new { _id = transactionId });
        }*/

        /*
        private async Task ProcessTransactionAsync(string transactionId)
        {
            // Create a new scope for the entire operation
            using var scope = _serviceScopeFactory.CreateScope();
            var propertyService = scope.ServiceProvider.GetRequiredService<IPropertyService>();

            try
            {
                // Retrieve the transaction and related property info
                var transaction = await propertyService.GetTransactionByIdAsync(transactionId).ConfigureAwait(false);
                if (transaction == null)
                {
                    throw new Exception("Transaction not found.");
                }

                var propertyInfo = await propertyService.GetPropertyInfoAsync(transaction.Car).ConfigureAwait(false);

                // Create the result payload
                var resultPayload = new
                {
                    _id = transactionId,
                    dataset = new
                    {
                        type = "PROPERTY_INFO",
                        status = "COMPLETED",
                        results = propertyInfo
                    }
                };

                // Send the result to the client's webhook using RabbitMQ
                SendToRabbitMQ(transaction.CallbackUrl, resultPayload);
            }
            catch (Exception ex)
            {
                // Log the exception (implementation of logging is assumed)
                // Logger.LogError(ex, "Error processing transaction {TransactionId}", transactionId);
            }
        }

        private void SendToRabbitMQ(string callbackUrl, object payload)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: callbackUrl,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            Console.WriteLine(callbackUrl);

            // Criação do payload compatível com a estrutura esperada
            var messagePayload = new
            {
                CallbackUrl = callbackUrl,
                Data = payload
            };

            var message = JsonSerializer.Serialize(messagePayload);
            var body = Encoding.UTF8.GetBytes(message);


            channel.BasicPublish(exchange: "",
                                 routingKey: callbackUrl,
                                 basicProperties: null,
                                 body: body);
        }
        */
    }
}
