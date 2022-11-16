using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Jenner.Carteira.Agendador.Worker.Providers;
using Jenner.Comum;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jenner.Carteira.Agendador.Worker.Services.Consumer
{
    public class AgendadorWorker : KafkaConsumerBase
    {
        private readonly ISender _sender;
        private readonly ILogger<AgendadorWorker> _logger;

        public AgendadorWorker(IServiceProvider serviceProvider,  ISender sender, ILogger<AgendadorWorker> logger) : base(serviceProvider, new JsonEventFormatter<Comum.Models.Carteira>())
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task DoScopedAsync(CancellationToken cancellationToken)
        {
            if (KafkaConsumer is null)
            {
                throw new ArgumentException("For some reason the Consumer is null, this shouldn't happen.");
            }

            _logger.LogDebug("Subscribing to topic {kafkaTopic}", Constants.CloudEvents.AplicadaTopic);
            KafkaConsumer.Subscribe(Constants.CloudEvents.AplicadaTopic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Cancellation Status: {cancelStatus}", cancellationToken.IsCancellationRequested);
                    _logger.LogDebug("Waiting to Consume from topic...");
                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);
                    
                    _logger.LogDebug("Message received! Parsing to CloudEvent and working on it");
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);

                    if (cloudEvent.Data is Comum.Models.Carteira mensagem)
                    {
                        _ = CriaCarteiraAsync(mensagem, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("Message isn't a carteira but instead a {messageType}", cloudEvent.Type);
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

        private async Task CriaCarteiraAsync(Comum.Models.Carteira mensagem, CancellationToken cancellationToken)
        {
            try
            {
                CarteiraCreate carteiraCreate = new CarteiraCreate
                {
                    Id = mensagem.Id,
                    Cpf = mensagem.Cpf,
                    NomePessoa = mensagem.NomePessoa,
                    DataNascimento = mensagem.DataNascimento,
                    UltimaAplicacao = mensagem.GetLatestAplicacao()
                };

                await _sender.Send(carteiraCreate, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected behavior while creating carteira: {errorMessage}", e.Message);
            }
        }
    }
}
