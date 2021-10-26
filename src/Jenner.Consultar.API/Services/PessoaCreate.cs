using CloudNative.CloudEvents;
using Confluent.Kafka;
using Jenner.Consultar.API.Providers;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Consultar.API.Services
{
    public class PessoaCreate : IRequest<string>
    {
        public string Teste { get; set; }
        public PessoaCreate(string teste)
        {
            Teste = teste;
        }
    }

    public class PessoaCreateHandler : KafkaPublisherBase, IRequestHandler<PessoaCreate, string>
    {
        public IHttpContextAccessor HttpContextAccessor { get; }
        public PessoaCreateHandler(IHttpContextAccessor httpContextAccessor, IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter) : base(producer, cloudEventFormatter, "jenner")
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<string> Handle(PessoaCreate request, CancellationToken cancellationToken)
        {
            var requestSource = HttpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(HttpContextAccessor));

            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = "string enviada",
                Source = new UriBuilder(requestSource).Uri,
                Data = request.Teste
            };
            await PublishToKafka(cloudEvent, cancellationToken);

            return await Task.FromResult<string>(request.Teste);
        }
    }
}