using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Jenner.Aplicacao.API.Providers;
using Jenner.Comum;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Aplicacao.API.Services.Consumer
{
    public class AplicacaoWorker : KafkaConsumerBase
    {
        public ISender sender;
        public AplicacaoWorker(IServiceProvider serviceProvider, ISender sender) :
            base(serviceProvider, new JsonEventFormatter<string>())
        {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        protected override async Task DoScopedAsync(CancellationToken cancellationToken)
        {
            if (KafkaConsumer is null)
            {
                throw new ArgumentException("For some reason the Consumer is null, this shouldn't happen.");
            }

            KafkaConsumer.Subscribe(Constants.CloudEvents.AplicarTopic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);
                    if (cloudEvent.Data is AplicacaoCreate mensagem)
                    {
                        try
                        {
                            await sender.Send(cloudEvent.Data as AplicacaoCreate);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Não recebi uma aplicacao {e.Message}");
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
