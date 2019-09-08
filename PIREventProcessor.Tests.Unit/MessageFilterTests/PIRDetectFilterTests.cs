using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using PIREventProcessor.Influx;
using PIREventProcessor.Kafka;
using PIREventProcessor.MessageActionFilters;
using Proto.Models;

namespace PIREventProcessor.Tests.Unit.MessageFilterTests
{
    [TestFixture]
    public class PIRDetectFilterTests
    {
        private PIRDetectFilter pirDetectFilter;

        private Mock<IOptions<StationConfig>> _mockStationConfig = new Mock<IOptions<StationConfig>>();

        private Mock<IKafkaClient> _mockKafkaClient;

        private Mock<IInfluxClient> _mockInfluxClient;

        private Mock<ILogger<PIRDetectFilter>> _mockLogger = new Mock<ILogger<PIRDetectFilter>>();

        [SetUp]
        public void Setup()
        {
            _mockInfluxClient = new Mock<IInfluxClient>();

            _mockKafkaClient = new Mock<IKafkaClient>();

            _mockStationConfig.Setup(x => x.Value).Returns(new StationConfig()
            {
                Stations = new Station[] {new Station(){
                    Id="5677",
                    Description="garden",
                    Enabled=true
                } }
            });

            pirDetectFilter = new PIRDetectFilter(_mockLogger.Object, _mockStationConfig.Object, _mockKafkaClient.Object, _mockInfluxClient.Object);
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
    }
}