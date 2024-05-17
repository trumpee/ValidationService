﻿using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Trumpee.MassTransit;
using Trumpee.MassTransit.Configuration;
using ValidationService;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = CreateHostBuilder(args).Build();

Console.WriteLine("Done!");
await host.RunAsync();
return;

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration(config =>
        {
            config.AddEnvironmentVariables();
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        })
        .UseSerilog()
        .ConfigureServices((host, services) =>
        {
            services.AddScoped<OffensiveContentValidator>();
            services.AddScoped<OffenciveContentValidationService>();

            var rabbitTopologyBuilder = new RabbitMqTransportConfigurator();
            rabbitTopologyBuilder.AddExternalConfigurations(x =>
            {
                x.AddConsumer<ValidationConsumer>();
            });

            rabbitTopologyBuilder.UseExternalConfigurations((ctx, cfg) =>
            {
                cfg.ReceiveEndpoint("validate", e =>
                {
                    e.BindQueue = true;
                    e.PrefetchCount = 16;

                    e.UseConcurrencyLimit(4);
                    e.ConfigureConsumer<ValidationConsumer>(ctx);
                });
            });

            services.AddConfiguredMassTransit(host.Configuration, rabbitTopologyBuilder);
        });
}
