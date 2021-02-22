using EventProcessor.Influx;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;

namespace KafkaConsumer.Tests
{
    [TestFixture]
    public class InfluxClientTests
    {
        private  Mock<ILogger<InfluxClient>> _mockLogger;
        private  Mock<IOptions<InfluxConfig>> _mockConfig;

        public void SetupMocks()
        {
            _mockConfig = new Mock<IOptions<InfluxConfig>>();

            _mockLogger = new Mock<ILogger<InfluxClient>>();

            _mockConfig.Setup(x => x.Value).Returns(new InfluxConfig
            {
                Database = "EVENTS",
                InfluxServer = "http://192.168.0.83:8086"
            });
        }

        [Test]
        [Category("Integration")]
        public void test_that_writes_work()
        {
            SetupMocks();

            var influxClient = new InfluxClient(_mockConfig.Object, _mockLogger.Object);

            influxClient.WritePirDetectEvent("test1", "5677", "front garden", DateTime.Now);
        }

        [Test]
        [Category("Integration")]
        public void test_pir_voltage_write()
        {
            SetupMocks();

            var influxClient = new InfluxClient(_mockConfig.Object, _mockLogger.Object);

            influxClient.WritePirVoltage("test1", "5677",4.543m, DateTime.Now);
        }
    }
}