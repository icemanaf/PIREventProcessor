using EventProcessor.Kafka;
using EventProcessor.MessageActionFilters;
using Microsoft.Extensions.Options;
using Proto.Models;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace EventProcessor
{
    public class App
    {
        private readonly IKafkaClient _kafkaClient;

        private readonly AppConfig _appConfig;

        private IEnumerable<IEventSink<KafkaMessage>> _filters;

        public IObservable<KafkaMessage> KafkaMessageStream { get; }

        public App(IKafkaClient client, IOptions<AppConfig> appConfig, IEnumerable<IEventSink<KafkaMessage>> filters)
        {
            _filters = filters;

            _appConfig = appConfig.Value;

            _kafkaClient = client;

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

            foreach(var filter in _filters)
            {
                filter.Observe(KafkaMessageStream);
            }

            _kafkaClient.Consume(_appConfig.KafkaBrokers, _appConfig.MainEventTopic, _appConfig.ConsumerGroup);
        }
    }
}