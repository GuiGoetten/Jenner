using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Jenner.Carteira.API.Providers;
using Jenner.Comum;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Carteira.API.Services.Consumer
{
    public class CarteiraAplicadaWorker : KafkaConsumerBase
    {
        public ISender sender;
        public CarteiraAplicadaWorker(IServiceProvider serviceProvider, ISender sender) :
            base(serviceProvider, new JsonEventFormatter<Comum.Models.Carteira>())
        {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        protected override async Task DoScopedAsync(CancellationToken cancellationToken)
        {
            if (KafkaConsumer is null)
            {
                throw new ArgumentException("For some reason the Consumer is null, this shouldn't happen.");
            }

            KafkaConsumer.Subscribe(Constants.CloudEvents.AplicadaTopic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //TODO: Pra retirar depois
                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);

                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);

                    if (cloudEvent.Data is Comum.Models.Carteira mensagem)
                    {
                        try
                        {
                            CarteiraCreate carteiraCreate = new CarteiraCreate
                            {
                                Cpf = mensagem.Cpf,
                                NomePessoa = mensagem.NomePessoa,
                                DataNascimento = mensagem.DataNascimento,
                                DataAgendamento = mensagem.GetLatestAplicacao().DataAgendamento,
                                NomeVacina = mensagem.GetLatestAplicacao().NomeVacina,
                                Dose = mensagem.GetLatestAplicacao().Dose,
                                DataAplicada = mensagem.GetLatestAplicacao().DataAplicacao
                            };

                            await sender.Send(carteiraCreate, cancellationToken);

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Não rescebi uma aplicacao {e.Message}");
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occured: {e.Error.Reason}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

    }
}
