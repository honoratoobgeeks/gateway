using Infra.IoC;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using MassTransit;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.DTOs;
using Nest;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpClient();

var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var elasticSettings = new ConnectionSettings(new Uri(builder.Configuration["Elasticsearch:Url"]))
    .DefaultIndex(builder.Configuration["Elasticsearch:IndexName"]);

var elasticClient = new ElasticClient(elasticSettings);

// Adicionar o cliente Elasticsearch aos serviços
builder.Services.AddSingleton<IElasticClient>(elasticClient);

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.Message<TransactionDTO>(configTopology =>
        {
            configTopology.SetEntityName(TransactionDTO.Exchanger);

        });

        cfg.Publish<TransactionDTO>(publishConfig =>
        {
            publishConfig.ExchangeType = "fanout";
        });

        cfg.Message<SmsDTO>(configTopology =>
        {
            configTopology.SetEntityName(SmsDTO.Exchanger);

        });

        cfg.Publish<SmsDTO>(publishConfig =>
        {
            publishConfig.ExchangeType = "fanout";
        });

        cfg.Message<SmsWebhookDTO>(configTopology =>
       {
           configTopology.SetEntityName(SmsWebhookDTO.Exchanger); 
       });

        cfg.Publish<SmsWebhookDTO>(publishConfig =>
        {
            publishConfig.ExchangeType = "fanout";
        });

        cfg.Message<TransactionWebhookDTO>(configTopology =>
       {
           configTopology.SetEntityName(TransactionWebhookDTO.Exchanger); 
       });

        cfg.Publish<TransactionWebhookDTO>(publishConfig =>
        {
            publishConfig.ExchangeType = "fanout";
        });

    });
});



builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MediatorAPI"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddJaegerExporter(options =>
            {
                options.AgentHost = builder.Configuration["OpenTelemetry:Jaeger:AgentHost"];
                options.AgentPort = int.Parse(builder.Configuration["OpenTelemetry:Jaeger:AgentPort"]);
            });
    })
    .WithMetrics(metricsProviderBuilder =>
    {
        metricsProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MediatorAPI"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });



builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProxyWebhook API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});


var app = builder.Build();


app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "mediator";
    c.SwaggerEndpoint("/mediator/swagger/v1/swagger.json", "Name");
});

app.UsePathBase("/mediator");

app.UseSwagger();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();