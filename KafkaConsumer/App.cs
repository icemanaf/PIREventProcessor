using System;
using PIREventProcessor.Kafka;
using PIREventProcessor.Processor;
using Proto.Models;

namespace PIREventProcessor
{
    public class App
    {
        private readonly IKafkaClient _kafkaClient;

        private readonly IMessageProcessor _processor;

        public App(IKafkaClient client,IMessageProcessor processor)
        {
            _kafkaClient = client;

            _processor = processor;

            client.Consume();

            client.OnMessageReceived += Client_OnMessageReceived;
        }

        public  void Client_OnMessageReceived(object sender, KafkaMessage e)
        {
           //todo
        }

        public void Run()
        {
            Console.WriteLine("hello world");

            Console.Read();
        }
    }
}
