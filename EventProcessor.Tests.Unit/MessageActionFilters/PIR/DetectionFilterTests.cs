using EventProcessor.Influx;
using EventProcessor.MessageActionFilters;
using EventProcessor.MessageActionFilters.PIR;
using EventProcessor.Processor;
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
        private Mock<ITimeProvider> _mockTimeProvider;
        private Mock<IInfluxClient> _mockInfluxClient;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void test_that_when_config_is_enabled__the_detector_subscribes_into_the_message_stream()
        {
            _mockLogger = new Mock<ILogger<PIRDetectionFilter>>();
            _mockConfig = new Mock<IOptions<PIRDetectionFilterConfig>>();
            _mockStationConfig = new Mock<IOptions<StationConfig>>();
            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockInfluxClient = new Mock<IInfluxClient>();

            var testScheduler = new TestScheduler();

            var kmStream = testScheduler.CreateHotObservable<KafkaMessage>(new Recorded<Notification<KafkaMessage>>(0, Notification.CreateOnNext(new KafkaMessage() { Id = "id1" })));

            //setup
            _mockConfig.Setup(x => x.Value).Returns(new PIRDetectionFilterConfig { Enabled = true });

            var filter = new PIRDetectionFilter(_mockLogger.Object, _mockConfig.Object, _mockStationConfig.Object, _mockTimeProvider.Object, _mockInfluxClient.Object);

            filter.Observe(kmStream);

            _mockLogger.VerifyLogInfo("Detection filter  Enabled. Subscribing to Kafka message stream.");

            Assert.IsTrue(kmStream.Subscriptions.Count > 0);
        }

        [Test]
        public void test_that_when_threshold_is_exceeded_during_the_buffer_period__video_record_request_is_generated()
        {
            const string testStationId = "5677";
            const string testStationArea = "Area1";
            const string testKMPayLoad = "AAAADDD5677ZZZZ";

            //set the date and time for the test
            DateTime testDateTime = new DateTime(2020, 1, 1, 1, 0, 0);

            _mockLogger = new Mock<ILogger<PIRDetectionFilter>>();
            _mockConfig = new Mock<IOptions<PIRDetectionFilterConfig>>();
            _mockStationConfig = new Mock<IOptions<StationConfig>>();
            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockInfluxClient = new Mock<IInfluxClient>();

            _mockStationConfig.Setup(x => x.Value).Returns(new StationConfig()
            {
                Stations = new[] { new Station() { Id = testStationId, Description = testStationArea, Enabled = true } }
            });

            _mockTimeProvider.Setup(x => x.GetCurrentTimeUtc()).Returns(testDateTime);

            var testScheduler = new TestScheduler();

            var kmStream = testScheduler.CreateColdObservable<KafkaMessage>(
                testScheduler.OnNextAt(2000, GenerateTestKM(testKMPayLoad, testDateTime.AddMilliseconds(2000))),
                 testScheduler.OnNextAt(3000, GenerateTestKM(testKMPayLoad, testDateTime.AddMilliseconds(3000))),
                 testScheduler.OnNextAt(4000, GenerateTestKM(testKMPayLoad, testDateTime.AddMilliseconds(4000))),
                 testScheduler.OnNextAt(7000, GenerateTestKM(testKMPayLoad, testDateTime.AddMilliseconds(7000)))
                );

            //setup
            _mockConfig.Setup(x => x.Value).Returns(new PIRDetectionFilterConfig { Enabled = true });

            var filter = new PIRDetectionFilter(_mockLogger.Object, _mockConfig.Object, _mockStationConfig.Object, _mockTimeProvider.Object, _mockInfluxClient.Object, testScheduler);

            filter.Observe(kmStream);

            testScheduler.AdvanceByMs(10500);
            testScheduler.AdvanceByMs(1000);
            _mockLogger.VerifyLogInfo($"Activity detected in station {testStationId}");

            _mockInfluxClient.Verify(x => x.WritePirDetectEvent(It.IsAny<string>(), testStationId, testStationArea, It.IsAny<DateTime>()), Times.AtLeastOnce);
        }

        [Test]
        public void test_that_pir_data_older_than_1_min_are_discarded()
        {
            const string testStationId = "5677";
            const string testKMPayLoad = "AAAADDD5677ZZZZ";
            const string testStationArea = "Area1";

            //set the date and time for the test
            DateTime testDateTime = new DateTime(2020, 1, 1, 1, 0, 0);

            _mockLogger = new Mock<ILogger<PIRDetectionFilter>>();
            _mockConfig = new Mock<IOptions<PIRDetectionFilterConfig>>();
            _mockStationConfig = new Mock<IOptions<StationConfig>>();
            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockInfluxClient = new Mock<IInfluxClient>();

            _mockStationConfig.Setup(x => x.Value).Returns(new StationConfig()
            {
                Stations = new[] { new Station() { Id = testStationId, Description = testStationArea, Enabled = true } }
            });

            _mockTimeProvider.Setup(x => x.GetCurrentTimeUtc()).Returns(testDateTime.AddSeconds(150));

            var testScheduler = new TestScheduler();

            var kmStream = testScheduler.CreateColdObservable<KafkaMessage>(
                testScheduler.OnNextAt(2000, GenerateTestKM(testKMPayLoad, testDateTime.AddSeconds(70))),
                 testScheduler.OnNextAt(3000, GenerateTestKM(testKMPayLoad, testDateTime.AddSeconds(73))),
                 testScheduler.OnNextAt(4000, GenerateTestKM(testKMPayLoad, testDateTime.AddSeconds(74))),
                 testScheduler.OnNextAt(7000, GenerateTestKM(testKMPayLoad, testDateTime.AddSeconds(74)))
                );

            //setup
            _mockConfig.Setup(x => x.Value).Returns(new PIRDetectionFilterConfig { Enabled = true });

            var filter = new PIRDetectionFilter(_mockLogger.Object, _mockConfig.Object, _mockStationConfig.Object, _mockTimeProvider.Object, _mockInfluxClient.Object, testScheduler);

            filter.Observe(kmStream);

            testScheduler.AdvanceByMs(10500);
            testScheduler.AdvanceByMs(1000);
            _mockLogger.VerifyLogInfo($"Activity detected in station {testStationId}", Times.Never());

            _mockInfluxClient.Verify(x => x.WritePirDetectEvent(It.IsAny<string>(), testStationId, testStationArea, It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public void test_that_when_config_is_disabled__the_detector_does_not_subscribe_into_the_message_stream()
        {
            _mockLogger = new Mock<ILogger<PIRDetectionFilter>>();
            _mockConfig = new Mock<IOptions<PIRDetectionFilterConfig>>();
            _mockStationConfig = new Mock<IOptions<StationConfig>>();
            _mockTimeProvider = new Mock<ITimeProvider>();
            _mockInfluxClient = new Mock<IInfluxClient>();

            //setup
            _mockConfig.Setup(x => x.Value).Returns(new PIRDetectionFilterConfig { Enabled = false });

            Subject<KafkaMessage> kmStream = new Subject<KafkaMessage>();

            var filter = new PIRDetectionFilter(_mockLogger.Object, _mockConfig.Object, _mockStationConfig.Object, _mockTimeProvider.Object, _mockInfluxClient.Object);

            filter.Observe(kmStream);

            _mockLogger.VerifyLogInfo("Detection filter  disabled. Bypassing subscription to Kafka message stream");

            kmStream.OnNext(new KafkaMessage()
            {
            });

            Assert.IsFalse(kmStream.HasObservers);
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