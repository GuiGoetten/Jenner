using CloudNative.CloudEvents;
using Confluent.Kafka;
using Jenner.Agendamento.API.Data;
using Jenner.Agendamento.API.Providers;
using Jenner.Comum;
using Jenner.Comum.Models;
using Jenner.Comum.Models.Validators;
using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.Services
{
    public class AgendamentoCreate : IRequest<Aplicacao>
    {
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public string NomeVacina { get; set; }
        public int Dose { get; set; }
        public DateTime DataAgendamento { get; set; }
    }

    public class AgendamentoCreateHandler : KafkaPublisherBase, IRequestHandler<AgendamentoCreate, Aplicacao>
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private readonly IMongoDatabase MongoDatabase;

        public AgendamentoCreateHandler(IHttpContextAccessor httpContextAccessor, IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, IMongoDatabase mongoDatabase) :
            base(producer, cloudEventFormatter, Constants.CloudEvents.AgendadaTopic)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<Aplicacao> Handle(AgendamentoCreate request, CancellationToken cancellationToken)
        {
            Aplicacao aplicacaoAgendada = new(request.Cpf, request.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, null);

            aplicacaoAgendada.ValidaAgendamento();

            Carteira carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .FindOrCreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, aplicacaoAgendada, cancellationToken);

            var requestSource = HttpContextAccessor?.HttpContext?.Request.Host.Value ?? "FromAgendador";

            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.AgendadaType,
                Source = new UriBuilder(requestSource).Uri,
                Data = carteiraResult
            };

            //TODO: Fazer esse trem ficar assíncrono de verdade
            await PublishToKafka(cloudEvent, cancellationToken);

            return await Task.FromResult(aplicacaoAgendada);
        }

    }
}
