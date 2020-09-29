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

namespace EventProcessor.MessageActionFilters.PIR
{
    /// <summary>
    /// This filter detects when a PIR Detect message is broadcast by the RF gateway
    /// Example of a signal is AAAADDD4445ZZZZ , the digits in the middle are the sensor identifier which can be mapped to a location
    /// </summary>
    public class PIRDetectionFilter : IMessageActionFilter<KafkaMessage>
    {
        
        private Regex _pirDetectRegExp = new Regex("A{3,}D{3,}[0-9]{4,6}Z{3,}", RegexOptions.Compiled);
        private Regex _stationExtractorRegExp = new Regex("[0-9]{4,6}", RegexOptions.Compiled);

        private readonly ILogger<PIRDetectionFilter> _logger;
        private readonly PIRDetectionFilterConfig _config;
        private readonly StationConfig _stationConfig;
        private readonly ITimeProvider _timeProvider;
        private readonly IInfluxClient _influxClient;

        private IScheduler _scheduler;

        private const int BUFFER_TIME = 10;

        //if there are hits beyond the threshold value within the buffer, send an acknowledgement.
        private const int THRESHOLD_VALUE = 3;

        public PIRDetectionFilter(ILogger<PIRDetectionFilter> logger, IOptions<PIRDetectionFilterConfig> config, IOptions<StationConfig> stationConfig, ITimeProvider timeProvider, IInfluxClient influxClient, IScheduler scheduler = null)
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

            if (kmList.Count > 0)
                _logger.LogInformation("PIR event detected..");

            try
            {
                var stations = kmList.GroupBy(x => ExtractStationFromKM(x, _stationConfig).Id).Where(grp => grp.Count() > THRESHOLD_VALUE).Select(x => x.Key);

                foreach (var station in stations)
                {
                    var sta = _stationConfig.Stations.Where(x => x.Id.Equals(station)).FirstOrDefault();

                    _influxClient.WritePirDetectEvent(Guid.NewGuid().ToString(), station.Trim(), sta.Description, _timeProvider.GetCurrentTimeUtc());

                    _logger.LogInformation($"Activity detected in station {station}");
                }
            }
            catch(Exception ex)
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

        private Station ExtractStationFromKM(KafkaMessage message, StationConfig config)
        {
            var match = _stationExtractorRegExp.Match(message.Payload);

            if (match.Success)
            {
                var station = (new List<Station>(config.Stations)).FirstOrDefault(x => x.Id == match.Value);

                return station;
            }

            return null;
        }

        public bool Enabled()
        {
            return _config.Enabled;
        }

        public void Observe(IObservable<KafkaMessage> observable)
        {
            if (_config.Enabled)
            {
                _logger.LogInformation("Detection filter  Enabled. Subscribing to Kafka message stream.");

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
                    .Buffer(TimeSpan.FromSeconds(BUFFER_TIME), TimeSpan.FromSeconds(1), sch).Subscribe(OnNext, OnError, OnCompleted);
            }
            else
            {
                _logger.LogInformation("Detection filter  disabled. Bypassing subscription to Kafka message stream");
            }
        }
    }
}