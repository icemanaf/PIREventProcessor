using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proto.Models;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;

namespace PIREventProcessor.Kafka
{
    public class KafkaClient : IKafkaClient
    {
        private readonly KafkaConfig _kafkaConfig;

        private readonly ILogger _logger;

        public KafkaClient(ILogger<KafkaClient> logger, IOptions<KafkaConfig> config)
        {
            _logger = logger;

            _kafkaConfig = config.Value;
        }

        public event EventHandler<KafkaMessage> OnMessageReceived;

        public void Consume()
        {
            var config = new Dictionary<string, object>
            {
                {"group.id", _kafkaConfig.ConsumerGroup},
                {"bootstrap.servers", _kafkaConfig.Broker},
                {"auto.commit.interval.ms", 5000}
            };

            using (var consumer = new Consumer<Null, byte[]>(config, null, new ByteArrayDeserializer()))
            {
                consumer.Subscribe(_kafkaConfig.DetectionTopic);

                _logger.LogInformation("Connected to kafka");

                consumer.OnMessage += Consumer_OnMessage;

                consumer.OnError += Consumer_OnError;

                while (true) consumer.Poll(100);
            }
        }

        public Message SendVideoRequestMessage(KafkaMessage km)
        {
            var config = new Dictionary<string, object>
            {
                {"bootstrap.servers", _kafkaConfig.Broker}
            };

            using (var producer = new Producer(config))
            {
                var task = producer.ProduceAsync(_kafkaConfig.VideoRequestTopic, null, Serialize(km));

                _logger.LogInformation("sent video request message {@m} to {t}",km, _kafkaConfig.VideoRequestTopic);

                return task.Result;
            }
        }

        private void Consumer_OnError(object sender, Error e)
        {
            _logger.LogError("Kafka consumer error {e}", e);
        }

        private void Consumer_OnMessage(object sender, Message<Null, byte[]> e)
        {
            var message = DeSerialize(e.Value);

            _logger.LogInformation("message received {@m}", message);

            if (OnMessageReceived != null)
                OnMessageReceived(this, message);
        }

        private KafkaMessage DeSerialize(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return Serializer.Deserialize<KafkaMessage>(ms);
            }
        }

        private byte[] Serialize(KafkaMessage km)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, km);

                return ms.ToArray();
            }
        }
    }
}