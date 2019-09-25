using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PIREventProcessor.Influx;
using PIREventProcessor.Kafka;
using Proto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PIREventProcessor.MessageActionFilters
{
    public class PIRDetectFilter : IMessageActionFilter
    {
        private readonly ILogger<PIRDetectFilter> _logger;

        private readonly IKafkaClient _kafkaClient;
        private readonly IInfluxClient _influxClient;
        private readonly List<Station> _stations;

        private Regex _pirDetectRegExp = new Regex("A{3,}D{3,}[0-9]{4,6}Z{3,}", RegexOptions.Compiled);
        private Regex _stationExtractorRegExp = new Regex("[0-9]{4,6}", RegexOptions.Compiled);

        public PIRDetectFilter(ILogger<PIRDetectFilter> logger, IOptions<StationConfig> stationConfig, IKafkaClient kafkaClient, IInfluxClient influxClient)
        {
            _logger = logger;

            _influxClient = influxClient;

            _kafkaClient = kafkaClient;

            _stations = new List<Station>(stationConfig.Value.Stations);
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