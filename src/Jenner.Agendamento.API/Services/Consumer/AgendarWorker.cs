using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Jenner.Agendamento.API.Providers;
using Jenner.Comum;
using Jenner.Comum.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.Services.Consumer
{
    public class AgendarWorker : KafkaConsumerBase
    {
        public ISender sender;
        public AgendarWorker(IServiceProvider serviceProvider, ISender sender) :
            base(serviceProvider, new JsonEventFormatter<string>())
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

            KafkaConsumer.Subscribe(Constants.CloudEvents.AgendarTopic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //TODO: Pra retirar depois
                    Console.WriteLine("Ouvindo...");


                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);
                    if (cloudEvent.Data is AgendamentoCreate mensagem)
                    {
                        try 
                        {
                            await sender.Send(cloudEvent.Data as AgendamentoCreate);
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
