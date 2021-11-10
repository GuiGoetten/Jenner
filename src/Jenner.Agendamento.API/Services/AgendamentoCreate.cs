using CloudNative.CloudEvents;
using Confluent.Kafka;
using Jenner.Agendamento.API.Providers;
using Jenner.Comum;
using Jenner.Comum.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.Services
{
    public class AgendamentoCreate : IRequest<Aplicacao>
    {
        public Aplicacao Aplicacao { get; set; }
        public AgendamentoCreate(Aplicacao aplicacao)
        {
            Aplicacao = aplicacao ?? throw new ArgumentNullException(nameof(aplicacao));
        }        
    }

    public class AgendamentoCreateHandler : KafkaPublisherBase, IRequestHandler<AgendamentoCreate, Aplicacao>
    {
        public IHttpContextAccessor HttpContextAccessor { get; }
        public AgendamentoCreateHandler(IHttpContextAccessor httpContextAccessor, IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter) : 
            base(producer, cloudEventFormatter, Constants.CloudEvents.AgendadaTopic)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<Aplicacao> Handle(AgendamentoCreate request, CancellationToken cancellationToken)
        {

            //TODO: Conversar com o banco, para tentar agendar a aplicação

            request.Aplicacao.Id = Guid.NewGuid();

            //TODO: Após isso, envia a aplicação para a fila de aplicações agendadas e retorna para o usuário o comprovante do agendamento (aplicação com o GUID preenchido)

            var requestSource = HttpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(HttpContextAccessor));

            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.CloudEvents.AgendadaType,
                Source = new UriBuilder(requestSource).Uri,
                Data = request.Aplicacao
            };

            //TODO: Fazer esse trem ficar assíncrono de verdade
            await PublishToKafka(cloudEvent, cancellationToken);

            return await Task.FromResult(request.Aplicacao);
        }
    }
}
