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
        private IHttpContextAccessor _httpContextAccessor { get; }
        private readonly IMongoDatabase _mongoDb;

        public CarteiraCreateHandler(IHttpContextAccessor httpContextAccessor, IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, IMongoDatabase mongoDatabase) :
            base(producer, cloudEventFormatter, Constants.CloudEvents.AplicadaTopic)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _mongoDb = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<Comum.Models.Carteira> Handle(CarteiraCreate request, CancellationToken cancellationToken)
        {
            Comum.Models.Aplicacao aplicacaoAplicada = new(request.Cpf, request.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, request.DataAplicada);
            
            aplicacaoAplicada.ValidaAplicacao();

            return await _mongoDb
                .GetCarteiraCollection()
                .CreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, aplicacaoAplicada, cancellationToken);
        }
    }
}
