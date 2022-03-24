using CloudNative.CloudEvents;
using Confluent.Kafka;
using Jenner.Carteira.API.Data;
using Jenner.Carteira.API.Providers;
using Jenner.Comum;
using Jenner.Comum.Models.Validators;
using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Carteira.API.Services
{
    public class CarteiraCreate : IRequest<Comum.Models.Carteira>
    {
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public string NomeVacina { get; set; }
        public int Dose { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime? DataAplicada { get; set; }
    }

    public class CarteiraCreateHandler : KafkaPublisherBase, IRequestHandler<CarteiraCreate, Comum.Models.Carteira>
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private readonly IMongoDatabase MongoDatabase;

        public CarteiraCreateHandler(IHttpContextAccessor httpContextAccessor, IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, IMongoDatabase mongoDatabase) :
            base(producer, cloudEventFormatter, Constants.CloudEvents.AplicadaTopic)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<Comum.Models.Carteira> Handle(CarteiraCreate request, CancellationToken cancellationToken)
        {

            Comum.Models.Carteira carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .FindOrCreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, cancellationToken);

            Comum.Models.Aplicacao aplicacaoAplicada = new(carteiraResult.Cpf, carteiraResult.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, request.DataAplicada);

            aplicacaoAplicada.ValidaAplicacao();

            carteiraResult = carteiraResult.AddAplicacao(aplicacaoAplicada);

            carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .UpdateAsync(carteiraResult.ToPersistence(), cancellationToken);

            return await Task.FromResult(carteiraResult);
        }
    }
}
