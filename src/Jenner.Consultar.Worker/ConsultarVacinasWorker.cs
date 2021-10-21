using CloudNative.CloudEvents;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;

namespace Jenner.Consultar.Worker
{
    class ConsultarVacinasWorker : ConsumeWorker
    {
        private readonly ISender sender;
        private readonly IConfiguration configuration;
        public ConsultarVacinasWorker(IServiceProvider serviceProvider, IConfiguration configuration, CloudEventFormatter formatter = null) : base(serviceProvider, formatter)
        {
            //this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(sender));
        }

        protected override void DoScoped(CancellationToken cancellationToken)
        {
            if (KafkaConsumer is null)
            {
                throw new ArgumentException("For some reason the Consumer is null, this shouldn't happen.");
            }

            KafkaConsumer.Subscribe("jenner");

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = KafkaConsumer.Consume(cts.Token);
                        Console.WriteLine($"Consumed message '{cr.Value}' at: '{cr.TopicPartitionOffset}'.");
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                KafkaConsumer.Close();
            }
            
        }
    }
}
