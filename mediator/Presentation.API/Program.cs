using Infra.IoC;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using MassTransit;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infra.Data.Context;

try
{
    var builder = WebApplication.CreateBuilder(args);


    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .WriteTo.Console()
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    // Configure Kestrel to read settings from appsettings.json
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



    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"]);
                h.Password(builder.Configuration["RabbitMQ:Password"]);
            });

            cfg.Message<WebhookMessageDTO>(configTopology =>
            {
                configTopology.SetEntityName("FitBank");
            });

            cfg.Publish<WebhookMessageDTO>(publishConfig =>
            {
                publishConfig.ExchangeType = "fanout";
            });

            cfg.ReceiveEndpoint("queue_1", e =>
            {
                e.Bind("FitBank", x =>
                {
                    x.ExchangeType = "fanout";
                });

                e.Handler<WebhookMessageDTO>(async context =>
                {
                    // Lógica para processar as mensagens
                    Console.WriteLine($"Queue 1 received: {context.Message.Data}");
                });
            });

            cfg.ReceiveEndpoint("queue_2", e =>
            {
                e.Bind("FitBank", x =>
                {
                    x.ExchangeType = "fanout";
                });

                e.Handler<WebhookMessageDTO>(async context =>
                {
                    Console.WriteLine($"Queue 2 received: {context.Message.Data}");
                });
            });

            cfg.ReceiveEndpoint("queue_3", e =>
            {
                e.Bind("FitBank", x =>
                {
                    x.ExchangeType = "fanout";
                });

                e.Handler<WebhookMessageDTO>(async context =>
                {
                    Console.WriteLine($"Queue 3 received: {context.Message.Data}");
                });
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
        c.SwaggerEndpoint("swagger/v1/swagger.json", "Name");
    });

    app.UsePathBase("/mediator");

    app.UseSwagger();

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
    Log.Information("MediatorAPI is running...");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}