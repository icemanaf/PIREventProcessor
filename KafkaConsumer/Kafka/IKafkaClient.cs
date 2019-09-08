using System;
using Proto.Models;
using Confluent.Kafka;

namespace PIREventProcessor.Kafka
{
    public interface IKafkaClient
    {
        event EventHandler<KafkaMessage> OnMessageReceived;

        void Consume();

        Message SendVideoRequestMessage(KafkaMessage km);
    }
}