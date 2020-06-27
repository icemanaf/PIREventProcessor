using EventProcessor.Kafka;
using EventProcessor.MessageActionFilters.PIR;
using EventProcessor.Processor;
using Microsoft.Extensions.Options;
using Proto.Models;
using System;
using System.Reactive.Linq;

namespace EventProcessor
{
    public class App
    {
        private readonly IKafkaClient _kafkaClient;

        private readonly IMessageProcessor _processor;

        private readonly DetectionFilter _pirDetectFilter;

        private readonly AppConfig _appConfig;

        public IObservable<KafkaMessage> KafkaMessageStream { get; }

        public App(IKafkaClient client, IMessageProcessor processor, DetectionFilter pirDetectFilter, IOptions<AppConfig> appConfig)
        {
            _appConfig = appConfig.Value;

            _kafkaClient = client;

            _processor = processor;

            _pirDetectFilter = pirDetectFilter;

            _processor.AddMessageFilter(_pirDetectFilter);

            KafkaMessageStream = Observable.FromEvent<EventHandler<KafkaMessage>, KafkaMessage>(handler =>
            {
                EventHandler<KafkaMessage> kmHandler = (sender, e) => handler(e);

                return kmHandler;
            },
            x =>
            {
                _kafkaClient.OnMessageReceived += x;
            }, y =>
            {
                _kafkaClient.OnMessageReceived -= y;
            });
        }

        public void Run()
        {
            _kafkaClient.Consume(_appConfig.KafkaBrokers, _appConfig.MainEventTopic, _appConfig.ConsumerGroup);
        }
    }
}