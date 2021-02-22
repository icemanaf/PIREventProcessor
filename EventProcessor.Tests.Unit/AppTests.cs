using EventProcessor.Kafka;
using EventProcessor.EventSinks;
using EventProcessor.EventSinks.PIR;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Proto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace EventProcessor.Tests.Unit
{
    [TestFixture]
    public class AppTests
    {
        private Mock<IKafkaClient> _mockKafkaClient;

        private Mock<PIRDetectionSink> _mockPirFilter;
        private Mock<IOptions<AppConfig>> _mockIoptionsAppConfig;
        private Mock<IEnumerable<IEventSink<KafkaMessage>>> _mockFilters;

        [Test]
        public void test_that_the_app_loads_up()
        {
            _mockKafkaClient = new Mock<IKafkaClient>();

            _mockPirFilter = new Mock<PIRDetectionSink>();

            _mockIoptionsAppConfig = new Mock<IOptions<AppConfig>>();

            _mockIoptionsAppConfig.Setup(x => x.Value).Returns(new AppConfig
            {
                ConsumerGroup = "cg",
                KafkaBrokers = "broker1",
                MainEventTopic = "topic"
            });

            _mockFilters = new Mock<IEnumerable<IEventSink<KafkaMessage>>>();

            _mockFilters.Setup(x => x.GetEnumerator()).Returns(Enumerable.Empty<IEventSink<KafkaMessage>>().GetEnumerator());

            var app = new App(_mockKafkaClient.Object, _mockIoptionsAppConfig.Object, _mockFilters.Object);

            app.Run();
        }

        [Test]
        public void test_that_the_app_responds_when_the_topic_sends_a_message()
        {
            _mockKafkaClient = new Mock<IKafkaClient>();

            _mockIoptionsAppConfig = new Mock<IOptions<AppConfig>>();

            _mockIoptionsAppConfig.Setup(x => x.Value).Returns(new AppConfig
            {
                ConsumerGroup = "cg",
                KafkaBrokers = "broker1",
                MainEventTopic = "topic"
            });

            _mockFilters = new Mock<IEnumerable<IEventSink<KafkaMessage>>>();

            _mockFilters.Setup(x => x.GetEnumerator()).Returns(Enumerable.Empty<IEventSink<KafkaMessage>>().GetEnumerator());

            var app = new App(_mockKafkaClient.Object, _mockIoptionsAppConfig.Object, _mockFilters.Object);

            app.Run();

            var eventRaised = false;

            app.KafkaMessageStream.Subscribe(x =>
            {
                eventRaised = true;
            }, y => { });

            _mockKafkaClient.Raise(x => x.OnMessageReceived += null, this, new KafkaMessage());

            Assert.IsTrue(eventRaised);
        }
    }
}