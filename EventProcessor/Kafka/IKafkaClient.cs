using Confluent.Kafka;
using Proto.Models;
using System;

namespace EventProcessor.Kafka
{
    public interface IKafkaClient
    {
        event EventHandler<KafkaMessage> OnMessageReceived;

        void Consume(string Brokers, string Topic, string ConsumerGroup);

        Message SendVideoRequestMessage(KafkaMessage km);

        void SendMessage(string Brokers, string Topic, string ConsumerGroup, KafkaMessage message);
    }
}