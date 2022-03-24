using CloudNative.CloudEvents;
using Confluent.Kafka;
using Jenner.Aplicacao.API.Data;
using Jenner.Aplicacao.API.Providers;
using Jenner.Comum;
using Jenner.Comum.Models;
using Jenner.Comum.Models.Validators;
using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Aplicacao.API.Services
{
    public class AplicacaoCreate : IRequest<Comum.Models.Aplicacao>
    {
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public string NomeVacina { get; set; }
        public int Dose { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime? DataAplicada { get; set; }
    }

    public class AplicacaoCreateHandler : KafkaPublisherBase, IRequestHandler<AplicacaoCreate, Comum.Models.Aplicacao>
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private readonly IMongoDatabase MongoDatabase;

        public AplicacaoCreateHandler(IHttpContextAccessor httpContextAccessor, IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, IMongoDatabase mongoDatabase) :
            base(producer, cloudEventFormatter, Constants.CloudEvents.AplicadaTopic)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<Comum.Models.Aplicacao> Handle(AplicacaoCreate request, CancellationToken cancellationToken)
        {

            //Comum.Models.Aplicacao aplicacaoAgendada = new(request.Cpf, request.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, request.DataAplicada);


            Carteira carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .FindOrCreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, cancellationToken);

            Comum.Models.Aplicacao aplicacaoAplicada = new(carteiraResult.Cpf, carteiraResult.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, request.DataAplicada);

            aplicacaoAplicada.ValidaAplicacao();

            carteiraResult = carteiraResult.AddAplicacao(aplicacaoAplicada);

            carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .UpdateAsync(carteiraResult.ToPersistence(), cancellationToken);

            //TODO: Após isso, envia a aplicação para a fila de aplicações agendadas e retorna para o usuário o comprovante do agendamento (aplicação com o GUID preenchido)

            var requestSource = HttpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(HttpContextAccessor));

            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.AplicadaType,
                Source = new UriBuilder(requestSource).Uri,
                Data = carteiraResult
            };

            //TODO: Fazer esse trem ficar assíncrono de verdade
            await PublishToKafka(cloudEvent, cancellationToken);

            return await Task.FromResult(aplicacaoAplicada);
        }
    }
}
