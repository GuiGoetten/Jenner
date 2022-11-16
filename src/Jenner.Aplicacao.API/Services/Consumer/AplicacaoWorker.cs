using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using DnsClient.Internal;
using Jenner.Aplicacao.API.Providers;
using Jenner.Comum;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Aplicacao.API.Services.Consumer
{
    public class AplicacaoWorker : KafkaConsumerBase
    {
        public ISender sender;
        private readonly ILogger<AplicacaoWorker> _logger;

        public AplicacaoWorker(IServiceProvider serviceProvider, ISender sender, ILogger<AplicacaoWorker> logger) :
            base(serviceProvider, new JsonEventFormatter<AplicacaoCreate>())
        {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task DoScopedAsync(CancellationToken cancellationToken)
        {
            if (KafkaConsumer is null)
            {
                throw new ArgumentException("For some reason the Consumer is null, this shouldn't happen.");
            }

            KafkaConsumer.Subscribe(Constants.CloudEvents.AplicarTopic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);
                    if (cloudEvent.Data is AplicacaoCreate mensagem)
                    {
                        await CriaAplicacaoAsync(mensagem, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError(e, "Error while parsing message, discarding it");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "A catastrophic error happened: {errorMessage}", ex.Message);
                    throw;
                }
            }
        }
        private async Task CriaAplicacaoAsync(AplicacaoCreate mensagem, CancellationToken cancellationToken)
        {
            try
            {
                await sender.Send(mensagem, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected behavior while creating aplicacao: {errorMessage}", e.Message);
            }
        }

    }
}
