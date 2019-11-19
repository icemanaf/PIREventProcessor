using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using PIREventProcessor.Influx;
using PIREventProcessor.Kafka;
using PIREventProcessor.MessageActionFilters;
using PIREventProcessor.Utilities;
using Proto.Models;
using System;

namespace PIREventProcessor.Tests.Unit.MessageFilterTests
{
    [TestFixture]
    public class PIRDetectFilterTests
    {
        private PIRDetectFilter pirDetectFilter;

        private Mock<IOptions<StationConfig>> _mockStationConfig = new Mock<IOptions<StationConfig>>();

        private Mock<IKafkaClient> _mockKafkaClient;

        private Mock<IInfluxClient> _mockInfluxClient;

        private Mock<ITimeProvider> _mockTimeProvider;

        private Mock<ILogger<PIRDetectFilter>> _mockLogger = new Mock<ILogger<PIRDetectFilter>>();

        [SetUp]
        public void Setup()
        {
            _mockInfluxClient = new Mock<IInfluxClient>();

            _mockKafkaClient = new Mock<IKafkaClient>();

            _mockTimeProvider = new Mock<ITimeProvider>();

            _mockStationConfig.Setup(x => x.Value).Returns(new StationConfig()
            {
                Stations = new Station[] {new Station(){
                    Id="5677",
                    Description="garden",
                    Enabled=true
                } }
            });

            pirDetectFilter = new PIRDetectFilter(_mockLogger.Object, _mockStationConfig.Object, _mockKafkaClient.Object, _mockInfluxClient.Object, _mockTimeProvider.Object);
        }

        [TestCase("ZZJZZAAAAAAAAADDDDD5677ZZZZ", 1)]
        [TestCase("ZQD000698AAAAAAAAKKKKK5677ZZZZ", 0)]
        [TestCase("ZZJZZAAAAAAAAADDDDD8677ZZZZ", 0)]
        public void test_that_for_a_valid_detection__influx_and_a_video_request_is_updated_and_sent(string detectString, int detection_freq)
        {
            Setup();

            const string msgId = "Id";

            var valid_message = new KafkaMessage()
            {
                Id = msgId,
                DatetimeCreatedUtc = new System.DateTime(2019, 10, 1).ToString(),
                MsgType = KafkaMessage.MessageType.Event,
                Payload = detectString
            };

            pirDetectFilter.Execute(valid_message);

            _mockKafkaClient.Verify(x => x.SendVideoRequestMessage(It.Is<KafkaMessage>(a => a.Id == msgId)), Times.Exactly(detection_freq));

            _mockInfluxClient.Verify(x => x.WritePirDetectEvent(msgId, "5677", "garden", It.IsAny<System.DateTime>()), Times.Exactly(detection_freq));
        }

        [Test]
        public void test_that_valid_influx_and_video_is_triggered_only_when_2_detect_events_happen_within_10_seconds()
        {
            Setup();

            var baseTime = new DateTime(2019, 1, 1, 10, 0, 0);

            _mockTimeProvider.Setup(x => x.GetCurrentTimeUtc()).Returns(baseTime);

            var km = GenerateKafKaMessage("id1", baseTime);

            pirDetectFilter.Execute(km);

            //should not trigger video event.
            _mockInfluxClient.Verify(x => x.WritePirDetectEvent(It.IsAny<String>(), "5677", "garden", It.IsAny<System.DateTime>()), Times.Never);

            baseTime = baseTime.AddSeconds(5);

            _mockTimeProvider.Setup(x => x.GetCurrentTimeUtc()).Returns(baseTime);

            km = GenerateKafKaMessage("id2", baseTime);

            pirDetectFilter.Execute(km);

            //should  trigger video event.
            _mockInfluxClient.Verify(x => x.WritePirDetectEvent(It.IsAny<String>(), "5677", "garden", It.IsAny<System.DateTime>()), Times.Once);
        }

        [Test]
        public void test_that_when_a_valid_event_is_detected_within__cooling_down_period_of_2_minutes_is_enforced()
        {
            Setup();

            var baseTime = new DateTime(2019, 1, 1, 10, 0, 0);

            _mockTimeProvider.Setup(x => x.GetCurrentTimeUtc()).Returns(baseTime);

            var km = GenerateKafKaMessage("id1", baseTime);

            pirDetectFilter.Execute(km);

            //should not trigger video event.
            _mockInfluxClient.Verify(x => x.WritePirDetectEvent(It.IsAny<String>(), "5677", "garden", It.IsAny<System.DateTime>()), Times.Never);

            baseTime = baseTime.AddSeconds(5);

            _mockTimeProvider.Setup(x => x.GetCurrentTimeUtc()).Returns(baseTime);

            km = GenerateKafKaMessage("id2", baseTime);

            pirDetectFilter.Execute(km);

            //should  trigger video event.
            _mockInfluxClient.Verify(x => x.WritePirDetectEvent(It.IsAny<String>(), "5677", "garden", It.IsAny<System.DateTime>()), Times.Once);

            baseTime = baseTime.AddSeconds(5);

            _mockTimeProvider.Setup(x => x.GetCurrentTimeUtc()).Returns(baseTime);

            km = GenerateKafKaMessage("id3", baseTime);

            pirDetectFilter.Execute(km);

            //should not trigger video event.
            _mockInfluxClient.Verify(x => x.WritePirDetectEvent(It.IsAny<String>(), "5677", "garden", It.IsAny<System.DateTime>()), Times.Once);

            baseTime = baseTime.AddMinutes(7);

            _mockTimeProvider.Setup(x => x.GetCurrentTimeUtc()).Returns(baseTime);

            km = GenerateKafKaMessage("id4", baseTime);

            pirDetectFilter.Execute(km);

            baseTime = baseTime.AddSeconds(4);

            _mockTimeProvider.Setup(x => x.GetCurrentTimeUtc()).Returns(baseTime);

            km = GenerateKafKaMessage("id5", baseTime);

            pirDetectFilter.Execute(km);

            //should  trigger video event.
            _mockInfluxClient.Verify(x => x.WritePirDetectEvent(It.IsAny<String>(), "5677", "garden", It.IsAny<System.DateTime>()), Times.Exactly(2));
        }

        private KafkaMessage GenerateKafKaMessage(string id, DateTime t) => new KafkaMessage()
        {
            Id = id,
            DatetimeCreatedUtc = t.ToString(),
            MsgType = KafkaMessage.MessageType.Event,
            Payload = "ZZJZZAAAAAAAAADDDDD5677ZZZZ"
        };
    }
}