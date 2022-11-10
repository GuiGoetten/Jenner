using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using DnsClient.Internal;
using Jenner.Agendamento.API.Providers;
using Jenner.Comum;
using Jenner.Comum.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.Services.Consumer
{
    public class AgendarWorker : KafkaConsumerBase
    {
        public ISender sender;
        private readonly ILogger<AgendarWorker> _logger;

        public AgendarWorker(IServiceProvider serviceProvider, ISender sender, ILogger<AgendarWorker> logger) :
            base(serviceProvider, new JsonEventFormatter<Comum.Models.Carteira>())
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

            _logger.LogDebug("Subscribing Agendar Worker to the topic {agendarTopic}", Constants.CloudEvents.AgendarTopic);
            KafkaConsumer.Subscribe(Constants.CloudEvents.AgendarTopic);

            _logger.LogDebug("Reading the topic {agendarTopic}", Constants.CloudEvents.AgendarTopic);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {

                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);
                    
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);

                    _logger.LogDebug("A new wild message appears");

                    if (cloudEvent.Data is Carteira carteira)
                    {
                        _logger.LogDebug("The message is a Carteira with the cpf {carteiraCpf}", carteira.Cpf);
                        try 
                        {
                            AgendamentoCreate novoAgendamento = new AgendamentoCreate
                            {
                                Cpf = carteira.Cpf,
                                NomePessoa = carteira.NomePessoa,
                                DataNascimento = carteira.DataNascimento,
                                DataAgendamento = carteira.GetLatestAplicacao().DataAgendamento,
                                NomeVacina = carteira.GetLatestAplicacao().NomeVacina,
                                Dose = carteira.GetLatestAplicacao().Dose
                            };
                            _logger.LogDebug("Creating a new Agendamento for cpf {carteiraCpf}", carteira.Cpf);
                            await sender.Send(novoAgendamento, cancellationToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "O valor recebido não é uma plicação válida");
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError(e, "Error occured");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
        }

    }
}
