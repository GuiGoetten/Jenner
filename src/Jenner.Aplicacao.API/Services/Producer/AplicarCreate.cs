using CloudNative.CloudEvents;
using Confluent.Kafka;
using Jenner.Aplicacao.API.Providers;
using Jenner.Comum.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Threading;
using System;
using Jenner.Comum;

namespace Jenner.Aplicacao.API.Services.Producer
{
    public class AplicarCreate : IRequest<AplicacaoCreate>
    {
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public string NomeVacina { get; set; }
        public int Dose { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime? DataAplicada { get; set; }
    }

    public class AplicarCreateHandler : KafkaPublisherBase, IRequestHandler<AplicarCreate, AplicacaoCreate>
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ILogger<AplicarCreateHandler> _logger;

        public AplicarCreateHandler(IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, IMongoDatabase mongoDatabase, ILogger<AplicarCreateHandler> logger) :
                                                                       base(producer, cloudEventFormatter, Constants.CloudEvents.AplicarTopic)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AplicacaoCreate> Handle(AplicarCreate request, CancellationToken cancellationToken)
        {
            AplicacaoCreate aplicacaoCreate = new AplicacaoCreate()
            {
                Cpf = request.Cpf,
                NomePessoa = request.NomePessoa,
                DataNascimento = request.DataNascimento,
                DataAgendamento = request.DataAgendamento,
                DataAplicada = request.DataAplicada,
                Dose = request.Dose,
                NomeVacina = request.NomeVacina
            };

            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.AplicarType,
                Source = new UriBuilder("fromAplicacao").Uri, //new UriBuilder($"From Agendador {DateTime.Now}").Uri,
                Data = aplicacaoCreate
            };

            _logger.LogDebug("Publishing a new aplicação for usuário {userCpf} to Kafka", aplicacaoCreate.Cpf);
            
            await PublishToKafka(cloudEvent, cancellationToken);

            return aplicacaoCreate;
        }
    }
}
