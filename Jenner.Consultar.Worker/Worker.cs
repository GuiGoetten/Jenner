using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Consultar.Worker
{
    public abstract class Worker : BackgroundService
    {
        protected readonly IServiceProvider serviceProvider;
        protected readonly CloudEventFormatter cloudEventFormatter;
        protected IConsumer<string, byte[]> KafkaConsumer { get; private set; } = null;

        public Worker(
            IServiceProvider serviceProvider,
            CloudEventFormatter formatter = null)
        {
            this.serviceProvider = serviceProvider;
            cloudEventFormatter = formatter ?? new JsonEventFormatter();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            KafkaConsumer = serviceProvider
                .CreateScope().ServiceProvider
                .GetRequiredService<IConsumer<string, byte[]>>();
            return Task.Run(() => DoScoped(stoppingToken), stoppingToken);  // TALVEZ PRECISE MUDAR AQUI
        }

        protected abstract void DoScoped(CancellationToken cancellationToken);

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            KafkaConsumer?.Dispose();
            base.Dispose();
        }
    }
}
