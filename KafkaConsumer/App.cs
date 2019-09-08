using PIREventProcessor.Kafka;
using PIREventProcessor.MessageActionFilters;
using PIREventProcessor.Processor;
using Proto.Models;
using System;

namespace PIREventProcessor
{
    public class App
    {
        private readonly IKafkaClient _kafkaClient;

        private readonly IMessageProcessor _processor;

        private readonly PIRDetectFilter _pirDetectFilter;

        public App(IKafkaClient client, IMessageProcessor processor, PIRDetectFilter pirDetectFilter)
        {
            _kafkaClient = client;

            _processor = processor;

            _pirDetectFilter = pirDetectFilter;

            _processor.AddMessageFilter(_pirDetectFilter);

            client.Consume();

            client.OnMessageReceived += Client_OnMessageReceived;
        }

        public void Client_OnMessageReceived(object sender, KafkaMessage m)
        {
            //todo
            _processor.ProcessMessages(m);
        }

        public void Run()
        {
            Console.WriteLine("hello world");

            Console.Read();
        }
    }
}