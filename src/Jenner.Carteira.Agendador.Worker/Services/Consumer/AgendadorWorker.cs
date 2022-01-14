using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Jenner.Carteira.Agendador.Worker.Providers;
using Jenner.Comum;
using Jenner.Comum.Models;
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
        public AgendadorWorker(IServiceProvider serviceProvider,  ISender sender) : base(serviceProvider, new JsonEventFormatter<Comum.Models.Carteira>())
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
                    Console.WriteLine("Ouvindo minha benga...");
                    Console.WriteLine(cancellationToken.IsCancellationRequested); 

                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);
                    Console.WriteLine("Ouvindo o consume...");

                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);
                    Console.WriteLine("cloudevent...");

                    if (cloudEvent.Data is Comum.Models.Carteira mensagem)
                    {
                        Console.WriteLine("É uma carteira...");
                        try
                        {
                            Console.WriteLine("Vamo criar povo...");
                            CarteiraCreate carteiraCreate = new CarteiraCreate
                            {
                                Id = mensagem.Id,
                                Cpf = mensagem.Cpf,
                                NomePessoa = mensagem.NomePessoa,
                                DataNascimento = mensagem.DataNascimento,
                                UltimaAplicacao = mensagem.GetLatestAplicacao()
                            };
                            Console.WriteLine("DEU BOA...");

                            await sender.Send(carteiraCreate, cancellationToken);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Não rescebi uma aplicacao {e.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Não é uma carteira...");
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
            Console.WriteLine("SAI DO WHILE...");

        }
    }
}
