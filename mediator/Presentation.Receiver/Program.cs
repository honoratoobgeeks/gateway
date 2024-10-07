using MassTransit;
using Application.DTOs;

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

         cfg.ReceiveEndpoint("SmsWebhookDTO_Receiver", e =>
        {
            e.Bind(SmsWebhookDTO.Exchanger, x =>
            {
                x.ExchangeType = "fanout";
            });

            e.Consumer<SmsReceiver>();
        });
    });
});

builder.Services.AddMassTransitHostedService();

var app = builder.Build();

app.Run();