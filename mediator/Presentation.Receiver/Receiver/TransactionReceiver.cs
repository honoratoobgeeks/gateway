using MassTransit;
using System.Threading.Tasks;
using Application.DTOs;

public class TransactionReceiver : IConsumer<WebhookMessageDTO>
{
    public Task Consume(ConsumeContext<WebhookMessageDTO> context)
    {
        // Executa um Console.WriteLine com a mensagem recebida
        Console.WriteLine($"Received message from queue_1: {context.Message.Data}");
        return Task.CompletedTask;
    }
}
