using DataGovernance.API.Configuration;
using DataGovernance.API.Features.Runs;
using DataGovernance.API.Features.Tools;
using DataGovernance.API.Services.Harness;
using DataGovernance.API.Services.Harness.BuiltinTools;
using DataGovernance.Application.Harness.Abstractions;
using DataGovernance.Domain.Repositories;
using DataGovernance.Infrastructure.Data;
using DataGovernance.Infrastructure.Data.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HarnessOptions>(builder.Configuration.GetSection(HarnessOptions.SectionName));
builder.Services.Configure<MessagingOptions>(builder.Configuration.GetSection(MessagingOptions.SectionName));

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var postgresConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=datagovernance;Username=postgres;Password=postgres";

builder.Services.AddDbContext<DataGovernanceDbContext>(options => options.UseNpgsql(postgresConnection));

var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);

builder.Services.AddScoped<IDataAssetRepository, DataAssetRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddMediatR(typeof(Program));

builder.Services.AddSingleton<IKernelFactory, KernelFactory>();
builder.Services.AddScoped<IAIHarnessService, AIHarnessService>();
builder.Services.AddScoped<IToolHarnessService, ToolHarnessService>();
builder.Services.AddScoped<IToolHandler, EchoToolHandler>();
builder.Services.AddScoped<IToolHandler, UtcNowToolHandler>();

builder.Services.AddHostedService<RunProcessorBackgroundService>();

var messagingOptions = builder.Configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>() ?? new MessagingOptions();
builder.Services.AddMassTransit(config =>
{
    config.SetKebabCaseEndpointNameFormatter();

    if (messagingOptions.Transport.Equals("rabbitmq", StringComparison.OrdinalIgnoreCase))
    {
        config.UsingRabbitMq((_, cfg) =>
        {
            cfg.Host(messagingOptions.RabbitMqHost, h =>
            {
                h.Username(messagingOptions.RabbitMqUsername);
                h.Password(messagingOptions.RabbitMqPassword);
            });
        });
    }
    else
    {
        config.UsingInMemory((_, _) => { });
    }
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("DataGovernance.API"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.MapControllers();
app.MapRunEndpoints();
app.MapToolEndpoints();
app.MapGet("/health", () => Results.Ok(new { status = "ok", timestamp = DateTimeOffset.UtcNow }));

app.Run();
