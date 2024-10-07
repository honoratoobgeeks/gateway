using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System.Text;
using MassTransit;
using Nest;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IElasticClient _elasticClient;
        private readonly ISmsService _smsService;
        public TransactionService(ITransactionRepository TransactionRepository, IHttpClientFactory httpClientFactory, IPublishEndpoint publishEndpoint, IElasticClient elasticClient, ISmsService smsService)
        {
            _repository = TransactionRepository;
            _httpClientFactory = httpClientFactory;
            _publishEndpoint = publishEndpoint;
            _elasticClient = elasticClient;
            _smsService = smsService;


        }

        public async Task<Guid> CreateTransactionAsync(TransactionDTO transactionDto)
        {
            try
            {
                var transactionId = Guid.NewGuid();
                var transaction = new Transaction
                {
                    Id = transactionId,
                    Type = transactionDto.Type,
                    Data = transactionDto.Data,
                    Endpoint = transactionDto.Endpoint,
                    EndpointType = transactionDto.EndpointType,
                    Exchanger = TransactionDTO.Exchanger
                };

                // Inclui o Id recém-criado no campo Data
                var transactionData = new
                {
                    Id = transactionId,
                    Data = transactionDto.Data
                };

                transaction.Data = JsonSerializer.Serialize(transactionData);

                await _repository.AddAsync(transaction);

                /* 

                Logica de integração com API Terceira

                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(transaction.Data, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (transactionDto.EndpointType.ToLower() == "post")
                {
                    response = await client.PostAsync(transactionDto.Endpoint, content);
                }
                else
                {
                    response = await client.GetAsync(transactionDto.Endpoint);
                }
                */

                var indexResponse = await _elasticClient.IndexDocumentAsync(transactionDto);
                if (!indexResponse.IsValid)
                {
                    Console.WriteLine($"Erro ao indexar no Elasticsearch: {indexResponse.OriginalException.Message}");
                }

                var transactionDetails = JsonSerializer.Deserialize<TransactionData>(transactionDto.Data);

                if (transactionDetails != null && transactionDetails.Amount > 1000)
                {
                    // Envie um SMS se o amount for maior que 1000
                    await _smsService.SendSmsAsync("+5585999102103", $"Alerta: Uma transação de valor {transactionDetails.Amount} foi realizada.");
                }

                return !transaction.Id.Equals(Guid.Empty) ? transaction.Id : Guid.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                transactionDto.Log = ex.Message;
                await _publishEndpoint.Publish(transactionDto);
                return Guid.Empty;
            }
        }
        public async Task<List<TransactionDTO>> SearchTransactionsAsync(string query)
        {
            var searchResponse = await _elasticClient.SearchAsync<TransactionDTO>(s => s
                .Query(q => q
                    .QueryString(qs => qs
                        .Query(query)
                    )
                )
            );

            return searchResponse.Documents.ToList();
        }

        public async Task HandleWebhookAsync(Guid transactionId, string webhookData, string sourceIp, string eventType, Dictionary<string, string> headers)
        {
            var transaction = await _repository.GetByIdAsync(transactionId);
            if (transaction != null)
            {
                try
                {
                    var message = new TransactionWebhookDTO
                    {
                        Data = webhookData,
                        TransactionId = transactionId,
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
            else
            {
                Console.WriteLine("Transação não encontrada.");
            }
        }

    }

    public class TransactionData
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
