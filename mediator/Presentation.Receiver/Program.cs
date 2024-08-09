using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Configura o MassTransit para conectar ao RabbitMQ e ouvir a queue_1
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:UserName"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        // Configura o recebimento de mensagens da queue_1
         cfg.ReceiveEndpoint("queue_1", e =>
        {
            // Bind entre queue_1 e o exchanger FitBank
            e.Bind("FitBank", x =>
            {
                x.ExchangeType = "fanout"; // Tipo do exchange
            });

            e.Consumer<TransactionReceiver>(); // Consumidor que irá processar as mensagens
        });
    });
});

builder.Services.AddMassTransitHostedService();

var app = builder.Build();

app.Run();