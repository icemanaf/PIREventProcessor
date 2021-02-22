using EventProcessor.EventSinks;
using EventProcessor.EventSinks.PIR;
using EventProcessor.Influx;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EventProcessor.Tests.Unit.EventSinks.PIR
{
    [TestFixture]
    public class AckSinkTests
    {
        private AckSink _ackSink;

        private Mock<ILogger<AckSink>> _mockLogger;

        private Mock<StationConfig> _mockStationConfig;

        private Mock<InfluxClient> _mockInfluxClient;

        [Test]
        [TestCase("ZB8FU0U40R00XR44A60HB6AAAAAAAAAKKKKK5677BBBBB4275ZZZZ",true)]
        [TestCase("'ZPBN000H0BCAAAAAAAAKKKKK5677BBBBB4275ZZZZ",true)]
        public void test_that_ack_filter_responds_to_a_valid_message(string message,bool messageIsValid)
        {
            _mockLogger = new();

            _mockStationConfig = new();

            _mockInfluxClient = new();

            _mockStationConfig.Setup(x => x.Stations).Returns(new Station[] { new Station {Id="5677",Description="garden",Enabled=true } });

            _ackSink = new(_mockLogger.Object, _mockStationConfig.Object, _mockInfluxClient.Object);
        }
    }
}