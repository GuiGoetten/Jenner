using CloudNative.CloudEvents;
using Confluent.Kafka;
using Jenner.Agendamento.API.Data;
using Jenner.Agendamento.API.Providers;
using Jenner.Comum;
using Jenner.Comum.Models;
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
        public string CPF { get; set; }

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

            //TODO: Conversar com o banco, para tentar agendar a aplicação

            //request.Aplicacao.Id = Guid.NewGuid();
            var agendamentoPoco = new AplicacaoPersistence
            {
                CPF = request.CPF,
                DataAgendamento = request.DataAgendamento,
                Dose = request.Dose,
                NomeVacina = request.NomeVacina
            };

            await MongoDatabase
                .GetAplicacaoCollection()
                .InsertNewAsync(agendamentoPoco, cancellationToken);

            //TODO: Após isso, envia a aplicação para a fila de aplicações agendadas e retorna para o usuário o comprovante do agendamento (aplicação com o GUID preenchido)

            var requestSource = HttpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(HttpContextAccessor));

            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.AgendadaType,
                Source = new UriBuilder(requestSource).Uri,
                Data = request
            };

            //TODO: Fazer esse trem ficar assíncrono de verdade
            await PublishToKafka(cloudEvent, cancellationToken);

            return await Task.FromResult(agendamentoPoco.ToAplicacao());
        }
    }
}
