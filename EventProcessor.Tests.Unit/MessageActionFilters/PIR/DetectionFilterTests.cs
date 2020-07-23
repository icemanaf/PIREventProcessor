using EventProcessor.MessageActionFilters;
using EventProcessor.MessageActionFilters.PIR;
using EventProcessor.Processor;
using EventProcessor.Tests.Unit.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Proto.Models;
using System.Reactive.Subjects;

namespace EventProcessor.Tests.Unit.MessageActionFilters.PIR
{
    [TestFixture]
    public class DetectionFilterTests
    {
        private Mock<IMessageStreamer<KafkaMessage>> _mockMessageStreamer;
        private Mock<ILogger<PIRDetectionFilter>> _mockLogger;
        private Mock<IOptions<PIRDetectionFilterConfig>> _mockConfig;
        private Mock<IOptions<StationConfig>> _mockStationConfig;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void test_that_when_config_is_enabled__the_detector_subscribes_into_the_message_stream()
        {
            _mockMessageStreamer = new Mock<IMessageStreamer<KafkaMessage>>();
            _mockLogger = new Mock<ILogger<PIRDetectionFilter>>();
            _mockConfig = new Mock<IOptions<PIRDetectionFilterConfig>>();
            _mockStationConfig = new Mock<IOptions<StationConfig>>();

            //setup
            _mockConfig.Setup(x => x.Value).Returns(new PIRDetectionFilterConfig { Enabled = true });

            Subject<KafkaMessage> kmStream = new Subject<KafkaMessage>();
            _mockMessageStreamer.Setup(x => x.MessageSource).Returns(kmStream);

            var filter = new PIRDetectionFilter(_mockMessageStreamer.Object, _mockLogger.Object, _mockConfig.Object, _mockStationConfig.Object);

            _mockLogger.VerifyLogInfo("Detection filter  Enabled. Subscribing to Kafka message stream.");

            kmStream.OnNext(new KafkaMessage()
            {
            });

            Assert.IsTrue(kmStream.HasObservers);
        }

        [Test]
        public void test_that_when_config_is_disabled__the_detector_subscribes_into_the_message_stream()
        {
            _mockMessageStreamer = new Mock<IMessageStreamer<KafkaMessage>>();
            _mockLogger = new Mock<ILogger<PIRDetectionFilter>>();
            _mockConfig = new Mock<IOptions<PIRDetectionFilterConfig>>();
            _mockStationConfig = new Mock<IOptions<StationConfig>>();

            //setup
            _mockConfig.Setup(x => x.Value).Returns(new PIRDetectionFilterConfig { Enabled = false });

            Subject<KafkaMessage> kmStream = new Subject<KafkaMessage>();
            _mockMessageStreamer.Setup(x => x.MessageSource).Returns(kmStream);

            var filter = new PIRDetectionFilter(_mockMessageStreamer.Object, _mockLogger.Object, _mockConfig.Object, _mockStationConfig.Object);

            _mockLogger.VerifyLogInfo("Detection filter  disabled. Bypassing subscription to Kafka message stream");

            kmStream.OnNext(new KafkaMessage()
            {
            });

            Assert.IsFalse(kmStream.HasObservers);
        }


    }
}