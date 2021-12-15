using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Jenner.Carteira.Agendador.Worker.Providers;
using Jenner.Comum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Carteira.Agendador.Worker.Services.Consumer
{
    public class AgendadorWorker : KafkaConsumerBase
    {

        public ISender sender;
        public AgendadorWorker(IServiceProvider serviceProvider, ISender sender) : base(serviceProvider, new JsonEventFormatter<string>())
        {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        protected override async Task DoScopedAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Teste");
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
                    Console.WriteLine("Ouvindo...");


                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);
                    if (cloudEvent.Data is Comum.Models.Carteira mensagem)
                    {
                        try
                        {
                            CarteiraCreate carteiraCreate = new CarteiraCreate
                            {
                                Id = mensagem.Id,
                                Cpf = mensagem.Cpf,
                                NomePessoa = mensagem.NomePessoa,
                                DataNascimento = mensagem.DataNascimento,
                                UltimaAplicacao = mensagem.GetLatestAplicacao()
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
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
        }
    }
}
