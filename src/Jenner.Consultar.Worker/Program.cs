using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Jenner.Consultar.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>

            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration ?? throw new ArgumentException("Configurations weren't set for this worker, unable to continue");

                    services.AddScoped(c =>
                    {
                        var config = new ConsumerConfig
                        {
                            BootstrapServers = configuration.GetConnectionString(@"KafkaBootstrap"),
                            GroupId = "consultar-worker",
                            AutoOffsetReset = AutoOffsetReset.Earliest
                        };
                        return new ConsumerBuilder<string, byte[]>(config).Build();
                    });
                    services.AddHostedService<ConsultarVacinasWorker>();
                });
    }
}
