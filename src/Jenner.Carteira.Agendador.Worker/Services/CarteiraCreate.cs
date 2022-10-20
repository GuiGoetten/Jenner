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
        private readonly IMongoDatabase MongoDatabase;

        public CarteiraCreateHandler(IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, IMongoDatabase mongoDatabase) :
                                                                       base(producer, cloudEventFormatter, Constants.CloudEvents.AgendarTopic)
        {
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<Unit> Handle(CarteiraCreate request, CancellationToken cancellationToken)
        {
            Vacina vacinaResult = await MongoDatabase
                                        .GetVacinaCollection()
                                        .FindOrCreateAsync(request.UltimaAplicacao.NomeVacina, cancellationToken);

            Aplicacao novoAgendamento = new(request.Cpf, request.NomePessoa, request.UltimaAplicacao.NomeVacina, request.UltimaAplicacao.Dose + 1, ((DateTime)request.UltimaAplicacao.DataAplicacao).AddDays(vacinaResult.Intervalo), null);

            if (request.UltimaAplicacao.Dose >= vacinaResult.Doses)
            {
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

            await PublishToKafka(cloudEvent, cancellationToken);

            return Unit.Value;
        }

    }
}
