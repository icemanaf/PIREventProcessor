using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit;
using NUnit.Framework;
using PIREventProcessor.Kafka;
using Proto.Models;

namespace KafkaConsumer.Tests
{
    [TestFixture]
    public class KafkaClientTests
    {


        private  IKafkaClient _kafkaClient;

        private  Mock<ILogger<KafkaClient>> _mockLogger;

        private  Mock<IOptions<KafkaConfig>> _mockKafkaConfig;

        [SetUp]
        public void SetUp()
        {

            _mockLogger=new Mock<ILogger<KafkaClient>>();

            _mockKafkaConfig=new Mock<IOptions<KafkaConfig>>();

            _mockKafkaConfig.Setup(x => x.Value).Returns(new KafkaConfig()
            {
               Broker= "192.168.0.85:9092",
                DetectionTopic= "PIR_DETECT",
                WriteBackTopic= "VIDEO_REQUEST",
                ConsumerGroup= 0
            });

            _kafkaClient=new KafkaClient(_mockLogger.Object,_mockKafkaConfig.Object);
            
        }

        [Test]
        public void WriteMessageToTopic()
        {
            var km = new KafkaMessage()
            {
                DatetimeCreatedUtc = $"{DateTime.UtcNow:dd-MMM-yyyy HH:mm}",
                Id = Guid.NewGuid().ToString(),
                Payload = "this is a test",
                MsgType = KafkaMessage.MessageType.Command,
                RetryCount = 0,
                Source = "PIREventProcessor"
            };


             var m=_kafkaClient.SendMessage(km, _mockKafkaConfig.Object.Value.WriteBackTopic);

            Assert.IsNotNull(m);

        }
    }
}
