using CloudNative.CloudEvents;
using Confluent.Kafka;
using Jenner.Carteira.Agendador.Worker.Providers;
using Jenner.Comum;
using Jenner.Comum.Models;
using MediatR;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;
using Jenner.Carteira.Agendador.Worker.Data;
using Microsoft.Extensions.Logging;

namespace Jenner.Carteira.Agendador.Worker.Services
{
    public class CarteiraCreate : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public Aplicacao UltimaAplicacao { get; set; }
    }

    public class CarteiraCreateHandler : KafkaPublisherBase, IRequestHandler<CarteiraCreate, Unit>
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ILogger<CarteiraCreateHandler> _logger;

        public CarteiraCreateHandler(IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, IMongoDatabase mongoDatabase, ILogger<CarteiraCreateHandler> logger) :
                                                                       base(producer, cloudEventFormatter, Constants.CloudEvents.AgendarTopic)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(CarteiraCreate request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handling creation for carteira {carteiraId} for Usuário {userCpf}", request.Id, request.Cpf);
            Vacina vacinaResult = await _mongoDatabase
                                        .GetVacinaCollection()
                                        .FindOrCreateAsync(request.UltimaAplicacao.NomeVacina, cancellationToken);
            
            _logger.LogDebug("We have a Vacina named {vacinaName}, creating a new Agendamento for Usuário {userCpf}", vacinaResult.NomeVacina, request.Cpf);
            Aplicacao novoAgendamento = new(request.Cpf, request.NomePessoa, request.UltimaAplicacao.NomeVacina, request.UltimaAplicacao.Dose + 1, ((DateTime)request.UltimaAplicacao.DataAplicacao).AddDays(vacinaResult.Intervalo), null);

            if (request.UltimaAplicacao.Dose >= vacinaResult.Doses)
            {
                _logger.LogDebug("Returning null, all valid doses have been applied");
                return Unit.Value;
            }

            Comum.Models.Carteira carteira = new Comum.Models.Carteira(request.Id, request.Cpf, request.NomePessoa, request.DataNascimento);
            Comum.Models.Carteira carteiraSend = carteira.AddAplicacao(novoAgendamento);

            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.AplicadaType,
                Source = new UriBuilder("fromAgendador").Uri, //new UriBuilder($"From Agendador {DateTime.Now}").Uri,
                Data = carteiraSend
            };

            _logger.LogDebug("Publishing a new aplicação for usuário {userCpf} to Kafka", carteiraSend.Cpf);
            _ = await PublishToKafka(cloudEvent, cancellationToken);
            return Unit.Value;
        }
    }
}
