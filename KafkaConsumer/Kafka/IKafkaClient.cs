using Confluent.Kafka;
using Proto.Models;
using System;

namespace PIREventProcessor.Kafka
{
    public interface IKafkaClient
    {
        event EventHandler<KafkaMessage> OnMessageReceived;

        void Consume();

        DeliveryResult<Null, byte[]> SendMessage(KafkaMessage km, string topic);
    }
}