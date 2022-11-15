using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Jenner.Carteira.API.Providers;
using Jenner.Comum;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Carteira.API.Services.Consumer
{
    public class CarteiraAgendadaWorker : KafkaConsumerBase
    {
        private readonly ISender _sender;
        private readonly ILogger<CarteiraAgendadaWorker> _logger;

        public CarteiraAgendadaWorker(IServiceProvider serviceProvider, ISender sender, ILogger<CarteiraAgendadaWorker> logger) :
            base(serviceProvider, new JsonEventFormatter<Comum.Models.Carteira>())
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task DoScopedAsync(CancellationToken cancellationToken)
        {
            if (KafkaConsumer is null)
            {
                throw new ArgumentNullException("For some reason the Consumer is null, this shouldn't happen.");
            }

            KafkaConsumer.Subscribe(Constants.CloudEvents.AgendadaTopic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //TODO: Pra retirar depois
                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);
                    if (cloudEvent.Data is Comum.Models.Carteira mensagem)
                    {
                        await CriaCarteiraAsync(mensagem, cancellationToken).ConfigureAwait(false);
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
                    Cpf = mensagem.Cpf,
                    NomePessoa = mensagem.NomePessoa,
                    DataNascimento = mensagem.DataNascimento,
                    DataAgendamento = mensagem.GetLatestAplicacao().DataAgendamento,
                    NomeVacina = mensagem.GetLatestAplicacao().NomeVacina,
                    Dose = mensagem.GetLatestAplicacao().Dose,
                    DataAplicada = mensagem.GetLatestAplicacao().DataAplicacao
                };

                await _sender.Send(carteiraCreate, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError("Error ocurred, the message is not of type Agendamento: ", e.Message);
            }
        }

    }
}
