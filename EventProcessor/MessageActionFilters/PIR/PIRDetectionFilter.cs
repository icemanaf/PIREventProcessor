using EventProcessor.Processor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proto.Models;
using System;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace EventProcessor.MessageActionFilters.PIR
{
    /// <summary>
    /// This filter detects when a PIR Detect message is broadcast by the RF gateway
    /// Example of a signal is AAAADDD4445ZZZZ , the digits in the middle are the sensor identifier which can be mapped to a location
    /// </summary>
    public class PIRDetectionFilter : IObserver<KafkaMessage>
    {
        private Regex _pirDetectRegExp = new Regex("A{3,}D{3,}[0-9]{4,6}Z{3,}", RegexOptions.Compiled);
        private Regex _stationExtractorRegExp = new Regex("[0-9]{4,6}", RegexOptions.Compiled);

        private readonly IMessageStreamer<KafkaMessage> _kmmMessageStreamer;
        private readonly ILogger<PIRDetectionFilter> _logger;
        private readonly PIRDetectionFilterConfig _config;
        private StationConfig _stationConfig;

        public PIRDetectionFilter(IMessageStreamer<KafkaMessage> streamer, ILogger<PIRDetectionFilter> logger, IOptions<PIRDetectionFilterConfig> config, IOptions<StationConfig> stationConfig)
        {
            _kmmMessageStreamer = streamer;

            _logger = logger;

            _config = config.Value;

            _stationConfig = stationConfig.Value;

            if (_config.Enabled)
            {
                _logger.LogInformation("Detection filter  Enabled. Subscribing to Kafka message stream.");

                _kmmMessageStreamer.MessageSource.Subscribe(OnNext, OnError);
            }
            else
            {
                _logger.LogInformation("Detection filter  disabled. Bypassing subscription to Kafka message stream");
            }
        }

        public void OnNext(KafkaMessage km)
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception e)
        {
            _logger.LogError(e.ToString());
        }

        public void OnCompleted()
        {
            _logger.LogInformation($"{nameof(this.GetType)}- stream has completed");
        }
    }
}