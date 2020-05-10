using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using EventProcessor.Influx;
using System;

namespace KafkaConsumer.Tests
{
    [TestFixture]
    public class InfluxClientTests
    {
        [Test]
        [Category("Integration")]
        public void test_that_writes_work()
        {
            var config = new Mock<IOptions<InfluxConfig>>();

            var logger = new Mock<ILogger<InfluxClient>>();

            config.Setup(x => x.Value).Returns(new InfluxConfig
            {
                Database = "EVENTS",
                InfluxServer = "http://192.168.0.83:8086"
            });

            var influxClient = new InfluxClient(config.Object, logger.Object);

            influxClient.WritePirDetectEvent("test1", "5677", "front garden", DateTime.Now);
        }
    }
}