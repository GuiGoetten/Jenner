using CloudNative.CloudEvents;
using Confluent.Kafka;
using DnsClient.Internal;
using Jenner.Aplicacao.API.Data;
using Jenner.Aplicacao.API.Providers;
using Jenner.Comum;
using Jenner.Comum.Models;
using Jenner.Comum.Models.Validators;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private IHttpContextAccessor _httpContextAccessor { get; }
        private readonly IMongoDatabase _mongoDb;
        private readonly ILogger<AplicacaoCreateHandler> _logger;

        public AplicacaoCreateHandler(IHttpContextAccessor httpContextAccessor,
            IProducer<string, byte[]> producer, 
            CloudEventFormatter cloudEventFormatter, 
            IMongoDatabase mongoDatabase,
            ILogger<AplicacaoCreateHandler> logger
            ) :
            base(producer, cloudEventFormatter, Constants.CloudEvents.AplicadaTopic)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _mongoDb = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Comum.Models.Aplicacao> Handle(AplicacaoCreate request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Recebido command para criar uma Aplicação: {userCpf}", request.Cpf);
            Comum.Models.Aplicacao aplicacaoAplicada = new(request.Cpf, request.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, request.DataAplicada);

            aplicacaoAplicada.ValidaAplicacao();
            _logger.LogDebug("Aplicacao validada: {userCpf)", request.Cpf);


            _logger.LogDebug("Criando uma carteira no banco");
            Carteira carteiraResult = await _mongoDb
                .GetCarteiraCollection()
                .CreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, aplicacaoAplicada, cancellationToken,cart =>
                {
                    if(cart is not null)
                    {
                        _logger.LogDebug("Carteira persistida com o ID {carteiraId} e Usuário {userCpf}", cart.Id, cart.Cpf);
                    }
                });


            //string requestSource = _httpContextAccessor?.HttpContext?.Request.Host.Value
            //                       ?? throw new ArgumentNullException(nameof(_httpContextAccessor));

            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.AplicadaType,
                Source = new UriBuilder("fromAplicacao").Uri,
                Data = carteiraResult
            };
            
            _logger.LogDebug("Enviando a carteira criada para o usuário {userCpf} para o serviço de mensageria", request.Cpf);

            await PublishToKafka(cloudEvent, cancellationToken);

            _logger.LogDebug("Carteira o usuário {userCpf} enviada para a mensageria", carteiraResult.Cpf);

            return aplicacaoAplicada;
        }
    }
}
