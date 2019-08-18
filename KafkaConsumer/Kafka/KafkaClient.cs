using Confluent.Kafka;
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
            var consumerconfig = new ConsumerConfig()
            {
                BootstrapServers = _kafkaConfig.Broker,
                GroupId = _kafkaConfig.ConsumerGroup.ToString(),
                AutoCommitIntervalMs = 5000,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var c = new ConsumerBuilder<Ignore, byte[]>(consumerconfig).Build())
            {
                c.Subscribe(_kafkaConfig.DetectionTopic);

                _logger.LogInformation("Connected to kafka");

                while (true)
                {
                    try
                    {
                        var cr = c.Consume();

                        ConsumerOnMessage(cr.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error {ex}", ex);
                    }
                }
            }
        }

        public DeliveryResult<Null, byte[]> SendMessage(KafkaMessage km, string topic)
        {
            var config = new Dictionary<string, object>
            {
                {"bootstrap.servers", _kafkaConfig.Broker}
            };

            var producerconfig = new ProducerConfig
            {
                BootstrapServers = _kafkaConfig.Broker
            };

            using (var p = new ProducerBuilder<Null, byte[]>(producerconfig).Build())
            {
                var task = p.ProduceAsync(topic, new Message<Null, byte[]>
                {
                    Value = Serialize(km)
                });

                return task.Result;
            }
        }

        private void ConsumerOnMessage(byte[] val)
        {
            var message = DeSerialize(val);

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