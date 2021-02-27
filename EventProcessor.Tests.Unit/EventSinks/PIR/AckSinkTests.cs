using EventProcessor.EventSinks;
using EventProcessor.EventSinks.PIR;
using EventProcessor.Influx;
using EventProcessor.Tests.Unit.Utilities;
using EventProcessor.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Proto.Models;
using ReactiveUI.Testing;
using System;
using System.Reactive;

namespace EventProcessor.Tests.Unit.EventSinks.PIR
{
    [TestFixture]
    public class AckSinkTests
    {
        private AckSink _ackSink;

        private Mock<ILogger<AckSink>> _mockLogger;

        private Mock<IOptions<StationConfig>> _mockStationConfig;

        private Mock<IInfluxClient> _mockInfluxClient;

        private Mock<IOptions<AckSinkConfig>> _mockConfig;

        private Mock<ITimeProvider> _mockTimeProvider;

        private const string testStationId = "5677";
        private const string testStationArea = "garden";

        [Test]
        public void test_that_when_config_is_enabled__the_sink_subscribes_into_the_message_stream()
        {
            _mockLogger = new();

            _mockStationConfig = new();

            _mockInfluxClient = new();

            _mockConfig = new();

            _mockStationConfig.Setup(x => x.Value).Returns(new StationConfig()
            {
                Stations = new Station[] { new Station() { Id = testStationId, Description = testStationArea, Enabled = true } }
            });

            _mockConfig.Setup(x => x.Value).Returns(new AckSinkConfig() { Enabled = true });

            _mockTimeProvider = new();

            _ackSink = new(_mockLogger.Object, _mockStationConfig.Object, _mockInfluxClient.Object, _mockConfig.Object, _mockTimeProvider.Object);

            var testScheduler = new TestScheduler();

            var kmStream = testScheduler.CreateHotObservable<KafkaMessage>(new Recorded<Notification<KafkaMessage>>(0, Notification.CreateOnNext(new KafkaMessage() { Id = "id1" })));

            _ackSink.Observe(kmStream);

            _mockLogger.VerifyLogInfo("Ack sink enabled.");

            Assert.IsTrue(kmStream.Subscriptions.Count > 0);
        }

        [Test]
        [TestCase("ZB8FU0U40R00XR44A60HB6AAAAAAAAAKKKKK5677BBBBB4275ZZZZ")]
        [TestCase("'ZPBN000H0BCAAAAAAAAKKKKK5677BBBBB4275ZZZZ")]
        public void test_that_ack_filter_responds_to_a_valid_message(string message)
        {
            _mockLogger = new();

            _mockStationConfig = new();

            _mockInfluxClient = new();

            _mockConfig = new();

            _mockTimeProvider = new();

            _mockStationConfig.Setup(x => x.Value).Returns(new StationConfig()
            {
                Stations = new Station[] { new Station { Id = testStationId, Description = testStationArea, Enabled = true } }
            });

            var testScheduler = new TestScheduler();
            DateTime testDateTime = new DateTime(2020, 1, 1, 1, 0, 0);

            var kmStream = testScheduler.CreateColdObservable<KafkaMessage>(
                testScheduler.OnNextAt(2000, GenerateTestKM(message, testDateTime.AddMilliseconds(2000)))

                );

            _mockConfig.Setup(x => x.Value).Returns(new AckSinkConfig() { Enabled = true });

            _ackSink = new(_mockLogger.Object, _mockStationConfig.Object, _mockInfluxClient.Object, _mockConfig.Object, _mockTimeProvider.Object, testScheduler);

            _ackSink.Observe(kmStream);

            testScheduler.AdvanceByMs(5000);

            _mockInfluxClient.Verify(x => x.WritePirVoltage(It.IsAny<string>(), testStationId, It.IsAny<float>(), It.IsAny<DateTime>()));
        }

        private KafkaMessage GenerateTestKM(string payload, DateTime messageGeneratedDateTime)
        {
            var ret = new KafkaMessage()
            {
                Id = (new Guid()).ToString(),
                DatetimeCreatedUtc = messageGeneratedDateTime.ToString("dd-MMM-yyyy HH:mm:ss"),
                Payload = payload,
                Source = "Test"
            };

            return ret;
        }
    }
}