using System;
using Proto.Models;
using Confluent.Kafka;

namespace EventProcessor.Kafka
{
    public interface IKafkaClient
    {
        event EventHandler<KafkaMessage> OnMessageReceived;

        void Consume();

        Message SendVideoRequestMessage(KafkaMessage km);
    }
}