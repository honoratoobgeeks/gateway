using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System.Text;
using MassTransit;
using AutoMapper;
using System.Text.Json;

namespace Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPublishEndpoint _publishEndpoint;

        public TransactionService(ITransactionRepository TransactionRepository, IHttpClientFactory httpClientFactory, IPublishEndpoint publishEndpoint)
        {
            _repository = TransactionRepository;
            _httpClientFactory = httpClientFactory;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> CreateTransactionAsync(TransactionDTO transactionDto)
        {
            var transactionId = Guid.NewGuid();
            var transaction = new Transaction
            {
                Id = transactionId,
                Type = transactionDto.Type,
                Data = transactionDto.Data,
                Endpoint = transactionDto.Endpoint,
                EndpointType = transactionDto.EndpointType,
                Exchanger = transactionDto.Exchanger
            };

            // Inclui o Id recém-criado no campo Data
            var transactionData = new
            {
                Id = transactionId,
                Data = transactionDto.Data
            };

            transaction.Data = JsonSerializer.Serialize(transactionData);

            await _repository.AddAsync(transaction);

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

            var isSuccess = response.IsSuccessStatusCode;
            return isSuccess ? transaction.Id : Guid.Empty;
        }

        public async Task HandleWebhookAsync(Guid transactionId, string webhookData)
        {
            var transaction = await _repository.GetByIdAsync(transactionId);
            if (transaction != null)
            {
                try
                {
                    var message = new WebhookMessageDTO
                    {
                        Exchanger = transaction.Exchanger,
                        Data = webhookData
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
}
