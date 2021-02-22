using EventProcessor.Influx;
using EventProcessor.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace EventProcessor.EventSinks.PIR
{
    /// <summary>
    /// This filter detects when a PIR Detect message is broadcast by the RF gateway
    /// Example of a signal is AAAADDD4445ZZZZ , the digits in the middle are the sensor identifier which can be mapped to a location
    /// </summary>
    public class PIRDetectionSink : IEventSink<KafkaMessage>
    {
        private Regex _pirDetectRegExp = new Regex("A{3,}D{3,}[0-9]{4,6}Z{3,}", RegexOptions.Compiled);
        private Regex _stationExtractorRegExp = new Regex("[0-9]{4,6}", RegexOptions.Compiled);

        private readonly ILogger<PIRDetectionSink> _logger;
        private readonly PIRDetectionSinkConfig _config;
        private readonly StationConfig _stationConfig;
        private readonly ITimeProvider _timeProvider;
        private readonly IInfluxClient _influxClient;

        private IScheduler _scheduler;

        private const int BUFFER_TIME = 10;

        //if there are hits beyond the threshold value within the buffer, send an acknowledgement.
        private const int THRESHOLD_VALUE = 1;

        public PIRDetectionSink(ILogger<PIRDetectionSink> logger, IOptions<PIRDetectionSinkConfig> config, IOptions<StationConfig> stationConfig, ITimeProvider timeProvider, IInfluxClient influxClient, IScheduler scheduler = null)
        {
            _logger = logger;

            _config = config.Value;

            _stationConfig = stationConfig.Value;

            _timeProvider = timeProvider;

            _influxClient = influxClient;

            _scheduler = scheduler;
        }

        public void OnNext(IList<KafkaMessage> kmList)
        {
            var x = kmList;

            try
            {
                var stations = kmList.Select((x) =>
                {
                    var pir_detection_match = _pirDetectRegExp.Match(x.Payload);

                    if (pir_detection_match.Success)
                    {
                        var pir_detect_string = pir_detection_match.Groups[0].Value;

                        var station_id_match = _stationExtractorRegExp.Match(pir_detect_string);

                        if (station_id_match.Success)
                        {
                            var station_id = station_id_match.Groups[0].Value;

                            return station_id;
                        }
                    }

                    return string.Empty;
                })
                    .Where(x => !string.IsNullOrEmpty(x))
                    .GroupBy(x => x)
                    .Where(grp => grp.Count() >= THRESHOLD_VALUE).
                    Select(x => x.Key);

                foreach (var station in stations)
                {
                    var sta = _stationConfig.Stations.Where(x => x.Id.Equals(station)).FirstOrDefault();

                    _influxClient.WritePirDetectEvent(Guid.NewGuid().ToString(), station.Trim(), sta.Description, _timeProvider.GetCurrentTimeUtc());

                    _logger.LogInformation($"Activity detected in station {station}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error!");
            }
        }

        public void OnError(Exception e)
        {
            _logger.LogError(e.ToString());
        }

        public void OnCompleted()
        {
            _logger.LogInformation("stream has completed.");
        }

        public bool Enabled()
        {
            return _config.Enabled;
        }

        public void Observe(IObservable<KafkaMessage> observable)
        {
            if (_config.Enabled)
            {
                _logger.LogInformation("PIR Detection filter  Enabled.");

                var sch = _scheduler == null ? DefaultScheduler.Instance : _scheduler;

                observable.Where(x =>
                {
                    var dt = Convert.ToDateTime(x.DatetimeCreatedUtc);

                    if ((_timeProvider.GetCurrentTimeUtc().Subtract(dt)).TotalSeconds > 60)
                        return false;

                    var ret = _pirDetectRegExp.Match(x.Payload).Success;

                    return ret; ;
                }).
                    Select(x => x)
                    .Buffer(TimeSpan.FromSeconds(BUFFER_TIME), TimeSpan.FromSeconds(2), sch).Subscribe(OnNext, OnError, OnCompleted);
            }
            else
            {
                _logger.LogInformation("Detection filter  disabled. Bypassing subscription to Kafka message stream");
            }
        }
    }
}