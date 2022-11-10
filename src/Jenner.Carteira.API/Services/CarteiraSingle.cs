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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Carteira.API.Services
{
    public class CarteiraSingle : IRequest<IEnumerable<Comum.Models.Carteira>>
    {
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
    }

    public class CarteiraSingleHandler : KafkaPublisherBase, IRequestHandler<CarteiraSingle, IEnumerable<Comum.Models.Carteira>>
    {
        private IHttpContextAccessor _httpContextAccessor { get; }
        private readonly IMongoDatabase _mongoDb;

        public CarteiraSingleHandler(IHttpContextAccessor httpContextAccessor, IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, IMongoDatabase mongoDb) :
            base(producer, cloudEventFormatter, Constants.CloudEvents.AplicadaTopic)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _mongoDb = mongoDb ?? throw new ArgumentNullException(nameof(mongoDb));
        }

        public async Task<IEnumerable<Comum.Models.Carteira>> Handle(CarteiraSingle request, CancellationToken cancellationToken)
        {
            return await _mongoDb
                .GetCarteiraCollection().GetOneAsync(request.Cpf, request.NomePessoa);

        }
    }
}
