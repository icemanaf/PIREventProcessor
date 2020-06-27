using EventProcessor.Kafka;
using EventProcessor.MessageActionFilters.PIR;
using EventProcessor.Processor;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Proto.Models;
using System;
using System.Reactive.Linq;

namespace EventProcessor.Tests.Unit
{
    [TestFixture]
    public class AppTests
    {
        private Mock<IKafkaClient> _mockKafkaClient;
        private Mock<IMessageProcessor> _mockMessageProcessor;
        private Mock<DetectionFilter> _mockPirFilter;
        private Mock<IOptions<AppConfig>> _mockIoptionsAppConfig;

        [Test]
        public void test_that_the_app_loads_up()
        {
            _mockKafkaClient = new Mock<IKafkaClient>();

            _mockMessageProcessor = new Mock<IMessageProcessor>();

            _mockPirFilter = new Mock<DetectionFilter>();

            _mockIoptionsAppConfig = new Mock<IOptions<AppConfig>>();

            _mockIoptionsAppConfig.Setup(x => x.Value).Returns(new AppConfig
            {
                ConsumerGroup = "cg",
                KafkaBrokers = "broker1",
                MainEventTopic = "topic"
            });

            var app = new App(_mockKafkaClient.Object, _mockMessageProcessor.Object, _mockPirFilter.Object, _mockIoptionsAppConfig.Object);

            _mockMessageProcessor.Verify(x => x.AddMessageFilter(It.IsAny<DetectionFilter>()), Times.Once);

            app.Run();
        }

        [Test]
        public void test_that_the_app_responds_when_the_topic_sends_a_message()
        {
            _mockKafkaClient = new Mock<IKafkaClient>();

            _mockMessageProcessor = new Mock<IMessageProcessor>();

            _mockPirFilter = new Mock<DetectionFilter>();

            _mockIoptionsAppConfig = new Mock<IOptions<AppConfig>>();

            _mockIoptionsAppConfig.Setup(x => x.Value).Returns(new AppConfig
            {
                ConsumerGroup = "cg",
                KafkaBrokers = "broker1",
                MainEventTopic = "topic"
            });

            var app = new App(_mockKafkaClient.Object, _mockMessageProcessor.Object, _mockPirFilter.Object, _mockIoptionsAppConfig.Object);

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