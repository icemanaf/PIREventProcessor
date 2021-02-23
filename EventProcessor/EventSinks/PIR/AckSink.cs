using EventProcessor.Influx;
using EventProcessor.Utilities;
using Microsoft.Extensions.Logging;
using Proto.Models;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

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
        private readonly AckSinkConfig _config;

        private IScheduler _scheduler;

        private Regex _ackStationRegExp = new Regex("A{3,}K{3,}[0-9]{3,4}B{3,}", RegexOptions.Compiled);

        private Regex _ackVoltageRegExp = new Regex("B{3,}[0-9]{4}Z{3,}", RegexOptions.Compiled);

        public AckSink(ILogger<AckSink> logger, StationConfig stationConfig, IInfluxClient influxClient, AckSinkConfig config)
        {
            _logger = logger;

            _influxClient = influxClient;

            _stationConfig = stationConfig;

            _config = config;
        }

        public bool Enabled()
        {
            return _config.Enabled;
        }

        public void Observe(IObservable<KafkaMessage> observable)
        {
            if (_config.Enabled)
            {
                _logger.LogInformation("Ack sink enabled.");

                var sch = _scheduler == null ? DefaultScheduler.Instance : _scheduler;

                observable.Where(x => true).
                    Select(x => x).Subscribe(onNext, onError, onCompleted);
             }
            else
            {
                _logger.LogInformation("Detection filter  disabled. Bypassing subscription to Kafka message stream");
            }
        }

        private void onCompleted()
        {
            throw new NotImplementedException();
        }

        private void onError(Exception obj)
        {
            throw new NotImplementedException();
        }

        private void onNext(KafkaMessage obj)
        {
            throw new NotImplementedException();
        }
    }
}