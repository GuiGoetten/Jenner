using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Jenner.Comum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using System.Reflection;

namespace Jenner.Carteira.Agendador.Worker
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
                    IConfiguration configuration = hostContext.Configuration ?? throw new ArgumentNullException("Configurations weren't set for this worker, unable to continue");

                    AddKafkaServices(services, configuration);

                    AddMongoServices(services, configuration);

                    services.AddMediatR(Assembly.GetExecutingAssembly());
                });



        private static void AddKafkaServices(IServiceCollection services, IConfiguration configuration)
        {

            // Producer

            services.AddSingleton(c =>
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = configuration.GetConnectionString(@"KafkaBootstrap")
                };
                return new ProducerBuilder<string, byte[]>(config).Build();
            });

            //Consumer

            services.AddScoped(c =>
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = configuration.GetConnectionString(@"KafkaBootstrap"),
                    GroupId = "agendador-worker",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };
                return new ConsumerBuilder<string, byte[]>(config).Build();
            });
            
            //Cloud Event Formatter

            services.AddSingleton<CloudEventFormatter>(new JsonEventFormatter());
        }

        private static void AddMongoServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(_ =>
            {
                return new MongoClient(configuration.GetConnectionString(Constants.MongoConnectionString));
            });
            services.AddScoped(sp =>
            {
                MongoClient mongoClient = sp.GetRequiredService<MongoClient>();
                return mongoClient.GetDatabase(Constants.MongoCarteiraDatabase);
            });
        }
    }
}
