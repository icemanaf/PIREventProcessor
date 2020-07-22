using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EventProcessor.Influx;
using EventProcessor.Kafka;
using EventProcessor.Utilities;
using Proto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EventProcessor.MessageActionFilters
{
    public class PIRDetectFilter  
    {
        private class StationHitCounter
        {
            public DateTime LastUpdated;

            public int counter;
        }

        private Dictionary<string, StationHitCounter> _stationStats = new Dictionary<string, StationHitCounter>();

        private readonly ILogger<PIRDetectFilter> _logger;

        private readonly IKafkaClient _kafkaClient;
        private readonly IInfluxClient _influxClient;
        private readonly List<Station> _stations;
        private readonly ITimeProvider _timeProvider;

        private const int detect_window_in_seconds = 10;
        private const int cooling_down_period_in_min = 2;
        private const int number_of_events = 2;

        private DateTime _last_video_detect_time = DateTime.MinValue;
        private DateTime _last_pir_event = DateTime.MinValue;
        private int _pir_counter = 0;

        private Regex _pirDetectRegExp = new Regex("A{3,}D{3,}[0-9]{4,6}Z{3,}", RegexOptions.Compiled);
        private Regex _stationExtractorRegExp = new Regex("[0-9]{4,6}", RegexOptions.Compiled);

        public PIRDetectFilter(ILogger<PIRDetectFilter> logger, IOptions<StationConfig> stationConfig, IKafkaClient kafkaClient, IInfluxClient influxClient, ITimeProvider timeProvider)
        {
            _logger = logger;

            _influxClient = influxClient;

            _kafkaClient = kafkaClient;

            _stations = new List<Station>(stationConfig.Value.Stations);

            _timeProvider = timeProvider;

            _stations.ForEach(x =>
            {
                _stationStats.Add(x.Id, new StationHitCounter()
                {
                    counter = 0,
                    LastUpdated = DateTime.MinValue
                });
            });
        }

        public void Execute(KafkaMessage km)
        {
            var pir_detection_match = _pirDetectRegExp.Match(km.Payload);

            if (pir_detection_match.Success)
            {
                //detect the station

                var pir_detect_string = pir_detection_match.Groups[0].Value;

                var station_id_match = _stationExtractorRegExp.Match(pir_detect_string);

                if (station_id_match.Success)
                {
                    var station_id = station_id_match.Groups[0].Value;

                    var station = _stations.FirstOrDefault(x => x.Id == station_id);

                    if (station != null)
                    {
                        if (_stationStats[station_id].LastUpdated.AddSeconds(detect_window_in_seconds) > _timeProvider.GetCurrentTimeUtc())
                        {
                            _stationStats[station_id].counter = _stationStats[station_id].counter + 1;

                            if (_stationStats[station_id].counter >= number_of_events)
                            {
                                if (_timeProvider.GetCurrentTimeUtc() > _last_video_detect_time.AddMinutes(cooling_down_period_in_min))
                                {
                                    var dt = Convert.ToDateTime(km.DatetimeCreatedUtc);
                                    //insert into influxdb
                                    _influxClient.WritePirDetectEvent(km.Id, station_id, station.Description, dt);

                                    //insert into the Video Request queue
                                    var vidRequestMessage = new KafkaMessage()
                                    {
                                        Id = km.Id,
                                        DatetimeCreatedUtc = DateTime.UtcNow.ToString(),
                                        MsgType = KafkaMessage.MessageType.Command,
                                        Payload = $"{{\"station\":{station_id}  }}",
                                        RetryCount = 1,
                                        Source = AppDomain.CurrentDomain.FriendlyName
                                    };

                                    _kafkaClient.SendVideoRequestMessage(km);

                                    _last_video_detect_time = _timeProvider.GetCurrentTimeUtc();
                                }
                            }
                        }

                        _stationStats[station_id].counter = 1;

                        _stationStats[station_id].LastUpdated = _timeProvider.GetCurrentTimeUtc();
                    }
                }
            }
        }

        public bool IsEnabled()
        {
            return true;
        }
    }
}