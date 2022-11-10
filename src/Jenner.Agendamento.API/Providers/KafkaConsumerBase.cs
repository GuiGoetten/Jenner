using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.Providers
{
    public abstract class KafkaConsumerBase : BackgroundService
    {
        protected readonly IServiceProvider serviceProvider;
        protected readonly CloudEventFormatter cloudEventFormatter;
        protected IConsumer<string, byte[]> KafkaConsumer { get; private set; } = null;

        public KafkaConsumerBase(
            IServiceProvider serviceProvider,
            CloudEventFormatter formatter = null)
        {
            this.serviceProvider = serviceProvider;
            cloudEventFormatter = formatter ?? new JsonEventFormatter();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            KafkaConsumer = serviceProvider
                .CreateScope().ServiceProvider
                .GetRequiredService<IConsumer<string, byte[]>>();
            await Task.Run(async () => await DoScopedAsync(stoppingToken), stoppingToken);  // TALVEZ PRECISE MUDAR AQUI
        }

        protected abstract Task DoScopedAsync(CancellationToken cancellationToken);

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            KafkaConsumer?.Dispose();
            base.Dispose();
        }
    }
}
