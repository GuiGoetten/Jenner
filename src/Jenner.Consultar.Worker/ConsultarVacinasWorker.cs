using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Consultar.Worker
{
    class ConsultarVacinasWorker : ConsumeWorker
    {
        public ConsultarVacinasWorker(
            IServiceProvider serviceProvider): base(serviceProvider, new JsonEventFormatter<string>())
        {
        }

        protected override async Task DoScopedAsync(CancellationToken cancellationToken)
        {
            if (KafkaConsumer is null)
            {
                throw new ArgumentException("For some reason the Consumer is null, this shouldn't happen.");
            }

            KafkaConsumer.Subscribe("jenner");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("Ouvindo...");

                    ConsumeResult<string, byte[]> result = KafkaConsumer.Consume(cancellationToken);
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);
                    if (cloudEvent.Data is string mensagem)
                    {
                        Console.WriteLine($"Consumed message '{mensagem}' at: '{cloudEvent.Type}'.");
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
