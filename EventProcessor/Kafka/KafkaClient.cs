using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using Proto.Models;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;

namespace EventProcessor.Kafka
{
    public class KafkaClient : IKafkaClient
    {
        private readonly ILogger _logger;

        public KafkaClient(ILogger<KafkaClient> logger)
        {
            _logger = logger;
        }

        public event EventHandler<KafkaMessage> OnMessageReceived;

        public void Consume(string Brokers, string Topic, string ConsumerGroup)
        {
            var config = new Dictionary<string, object>
            {
                {"group.id", ConsumerGroup},
                {"bootstrap.servers", Brokers},
                {"auto.commit.interval.ms", 5000}
            };

            using (var consumer = new Consumer<Null, byte[]>(config, null, new ByteArrayDeserializer()))
            {
                consumer.Subscribe(Topic);

                _logger.LogInformation("Connected to kafka");

                consumer.OnMessage += Consumer_OnMessage;

                consumer.OnError += Consumer_OnError;

                while (true) consumer.Poll(100);
            }
        }

        

        public void SendMessage(string Brokers, string Topic, string ConsumerGroup, KafkaMessage message)
        {
            throw new NotImplementedException();
        }

        public Message SendVideoRequestMessage(KafkaMessage km)
        {
            //var config = new Dictionary<string, object>
            //{
            //    {"bootstrap.servers", _kafkaConfig.Broker}
            //};

            //using (var producer = new Producer(config))
            //{
            //    var task = producer.ProduceAsync(_kafkaConfig.VideoRequestTopic, null, Serialize(km));

            //    _logger.LogInformation("sent video request message {@m} to {t}", km, _kafkaConfig.VideoRequestTopic);

            //    return task.Result;
            //}

            return null;
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