using MassTransit;
using System.Threading.Tasks;
using Application.DTOs;

public class SmsReceiver : IConsumer<SmsWebhookDTO>
{
    public Task Consume(ConsumeContext<SmsWebhookDTO> context)
    {
        // Executa um Console.WriteLine com a mensagem recebida
        Console.WriteLine($"Received message from RabbitMQ: {context.Message.Data}");
        return Task.CompletedTask;
    }
}
