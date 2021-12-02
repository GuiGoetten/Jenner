using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;
using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Aplicacao.API.Providers
{

    public class KafkaPublisherBase
        {
            private readonly IProducer<string, byte[]> producer;
            private readonly CloudEventFormatter cloudEventFormatter;
            private readonly string topic;

            /// <summary>
            /// Creates a new Publisher
            /// </summary>
            /// <param name="logger"></param>
            /// <param name="producer"></param>
            /// <param name="cloudEventFormatter"></param>
            /// <param name="topic"></param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            public KafkaPublisherBase( IProducer<string, byte[]> producer, CloudEventFormatter cloudEventFormatter, string topic)
            {
                if (string.IsNullOrEmpty(topic))
                {
                    throw new ArgumentException($"'{nameof(topic)}' cannot be null or empty.", nameof(topic));
                }

                this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
                this.cloudEventFormatter = cloudEventFormatter ?? throw new ArgumentNullException(nameof(cloudEventFormatter));
                this.topic = topic;
            }

            protected virtual Task<DeliveryResult<string, byte[]>> PublishToKafka(CloudEvent cloudEvent, CancellationToken cancellationToken)
            {

                try
                {
                    var result = producer.ProduceAsync(
                               topic,
                               cloudEvent.ToKafkaMessage(ContentMode.Structured, cloudEventFormatter),
                               cancellationToken);
                    return result;
                }
                catch
                {
                    throw;
                }
            }
        }
}
