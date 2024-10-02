using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Presentation.ThirdParty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThirdPartyController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ThirdPartyController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JsonElement data)
        {
            // Deserializar o JsonElement para TransactionData
            var transactionData = JsonSerializer.Deserialize<TransactionData>(data.GetRawText());

            // Simular lógica de processamento para o método POST
            var response = new { message = "Post request received", data };

            // Extrair a chave correta de transactionData.Id
            var webhookPayload = new
            {
                key = transactionData.Id,
                status = "completed",
                result = response
            };

            var content = new StringContent(JsonSerializer.Serialize(webhookPayload), Encoding.UTF8, "application/json");

            var webhookResponse = await _httpClient.PostAsync("https://localhost:5116/mediator/webhook", content);

            if (webhookResponse.IsSuccessStatusCode)
            {
                return Ok(response);
            }

            return StatusCode((int)webhookResponse.StatusCode, webhookResponse.ReasonPhrase);


        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Simular lógica de processamento para o método GET
            var response = new { message = "Get request received" };

            // Chamar o endpoint webhook de ProxyController
            var webhookPayload = new
            {
                key = "some-transaction-key", // Substitua pela lógica para obter a chave correta
                status = "completed",
                result = response
            };
            var content = new StringContent(JsonSerializer.Serialize(webhookPayload), Encoding.UTF8, "application/json");

            _httpClient.PostAsync("http://localhost:5116/mediator/webhook", content);

            return Ok(new { message = "Get request received" });

        }

    }

    public class TransactionData
    {
        public Guid Id { get; set; }
        public string Data { get; set; }
    }

}