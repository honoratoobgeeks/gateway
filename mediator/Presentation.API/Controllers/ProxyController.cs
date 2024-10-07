using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.DTOs;
using System.Text.Json;
using Automatonymous.Binders;
using Nest;

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
                var sourceIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                var eventType = "TransactionWebhookEvent";

                var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

                if (webhookData.TryGetProperty("key", out JsonElement keyElement) && keyElement.TryGetGuid(out Guid transactionId))
                {
                    await _transactionService.HandleWebhookAsync(transactionId, webhookData.ToString(), sourceIp, eventType, headers);

                    return Ok(webhookData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o webhook: {ex.Message}");
                return StatusCode(500, "Erro interno ao processar o webhook.");
            }

            return BadRequest("Chave de transação não encontrada ou inválida.");
        }

        [HttpGet("search")]
        [AllowAnonymous]

        public async Task<IActionResult> SearchTransactions([FromQuery] string query)
        {
            var results = await _transactionService.SearchTransactionsAsync(query);
            return Ok(results);
        }

    }
}
