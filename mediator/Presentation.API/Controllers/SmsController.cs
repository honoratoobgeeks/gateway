using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using System.Text.Json;

namespace Presentation.API.Controllers
{
    [Route("sms")]
    [ApiController]
    public class SmsController : ControllerBase
    {

        private readonly ISmsService _smsService;

        private readonly IConfiguration _configuration;

        public SmsController(IConfiguration configuration, ISmsService smsService)
        {
            _smsService = smsService;
            _configuration = configuration;
        }


        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] JsonElement webhookData)
        {

            try
            {
                var sourceIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                var eventType = "SmsWebhookEvent";

                var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

                await _smsService.HandleWebhookAsync(webhookData.ToString(), sourceIp, eventType, headers);
                
                Ok(webhookData);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return Ok(webhookData);

        }

        [HttpGet("search")]
        [AllowAnonymous]

        public async Task<IActionResult> SearchSms([FromQuery] string query)
        {
            var results = await _smsService.SearchSmsAsync(query);
            return Ok(results);
        }

    }
}
