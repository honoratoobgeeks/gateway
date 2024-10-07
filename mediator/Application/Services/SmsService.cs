using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MassTransit;
using Nest;
using Application.DTOs;
using Application.Interfaces;



namespace Application.Services
{
    public class SmsService : ISmsService
    {
        private readonly string _apiToken;
        private readonly string _requestUrl;
        private readonly string _externalId;
        private readonly string _from;
        private readonly HttpClient _httpClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IElasticClient _elasticClient; // Cliente Elasticsearch

        public SmsService(IConfiguration configuration, IHttpClientFactory httpClientFactory, IPublishEndpoint publishEndpoint, IElasticClient elasticClient)
        {
            _apiToken = configuration["Zenvia:ApiToken"];
            _requestUrl = configuration["Zenvia:RequestUrl"];
            _externalId = configuration["Zenvia:ExternalId"];
            _from = configuration["Zenvia:From"];
            _httpClient = httpClientFactory.CreateClient();
            _publishEndpoint = publishEndpoint;
            _elasticClient = elasticClient;

        }

        public async Task SendSmsAsync(string toPhoneNumber, string message)
        {
            var payload = new
            {
                externalId = _externalId,
                from = _from,
                to = toPhoneNumber,
                contents = new[]
                {
                new { type = "text", text = message }
            }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-TOKEN", $"{_apiToken}");

            var response = await _httpClient.PostAsync(_requestUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro ao enviar SMS: {errorMessage}");
            }
            else
            {
                Console.WriteLine("SMS enviado com sucesso.");

                var smsDTO = new SmsDTO
                {
                    Id = Guid.NewGuid(),
                    From = payload.from,
                    Message = message,
                    To = payload.to,
                    SentAt = DateTime.Now,

                };

                PublishSmsAsync(smsDTO);

            }
        }
        public async Task<Guid> PublishSmsAsync(SmsDTO smsDto)
        {

            try
            {
                await _publishEndpoint.Publish(smsDto);
                Console.WriteLine("Mensagem SMS publicada com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao publicar mensagem: {ex.Message}");
                smsDto.Log = ex.Message;
                _publishEndpoint.Publish(smsDto);
            }

            var indexResponse = await _elasticClient.IndexDocumentAsync(smsDto);
            if (!indexResponse.IsValid)
            {
                Console.WriteLine($"Erro ao indexar no Elasticsearch: {indexResponse.OriginalException.Message}");
            }

            return !smsDto.Id.Equals(Guid.Empty) ? smsDto.Id : Guid.Empty;
        }
        public async Task<List<SmsDTO>> SearchSmsAsync(string query)
        {
            var searchResponse = await _elasticClient.SearchAsync<SmsDTO>(s => s
                .Query(q => q
                    .QueryString(qs => qs
                        .Query(query)
                    )
                )
            );

            return searchResponse.Documents.ToList();
        }
        public async Task HandleWebhookAsync(string webhookData, string sourceIp, string eventType, Dictionary<string, string> headers)
        {
            try
            {
                var message = new SmsWebhookDTO
                {
                    Data = webhookData,
                    EventType = eventType,
                    SourceIP = sourceIp,
                    Headers = headers,
                    Timestamp = DateTime.UtcNow
                };

                await _publishEndpoint.Publish(message);
                Console.WriteLine("Mensagem publicada com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao publicar mensagem: {ex.Message}");
            }
        }

    }

}
