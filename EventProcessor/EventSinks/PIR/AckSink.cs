using EventProcessor.Influx;
using EventProcessor.Utilities;
using Microsoft.Extensions.Logging;
using Proto.Models;
using System;

namespace EventProcessor.EventSinks.PIR
{
    /// <summary>
    /// This sink identifies the 15m ack signals from the
    /// PIR sensor and records the voltage and sensor details in influxdb
    /// </summary>
    public class AckSink : IEventSink<KafkaMessage>
    {
        private readonly ILogger<AckSink> _logger;
        private readonly StationConfig _stationConfig;
        private readonly ITimeProvider _timeProvider;
        private readonly IInfluxClient _influxClient;

        public AckSink(ILogger<AckSink> logger, StationConfig stationConfig, IInfluxClient influxClient)
        {
            _logger = logger;

            _influxClient = influxClient;

            _stationConfig = stationConfig;
        }

        public bool Enabled()
        {
            return true;
        }

        public void Observe(IObservable<KafkaMessage> observable)
        {
            //throw new NotImplementedException();
        }
    }
}