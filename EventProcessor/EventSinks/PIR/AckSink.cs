using EventProcessor.Influx;
using EventProcessor.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proto.Models;
using System;
using System.Linq;
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

        private Regex _ackDetecRegExp = new Regex("A{3,}K{3,}[0-9]{3,4}B{3,}", RegexOptions.Compiled);

        private Regex _ackVoltageRegExp = new Regex("B{3,}[0-9]{4}Z{3,}", RegexOptions.Compiled);

        private Regex _stationExtractorRegExp = new Regex("[0-9]{4,6}", RegexOptions.Compiled);

        public AckSink(ILogger<AckSink> logger, IOptions<StationConfig> stationConfig, IInfluxClient influxClient, IOptions<AckSinkConfig> config, ITimeProvider timeProvider, IScheduler scheduler = null)
        {
            _logger = logger;

            _influxClient = influxClient;

            _stationConfig = stationConfig.Value;

            _config = config.Value;

            _timeProvider = timeProvider;

            _scheduler = scheduler;
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

                observable.Where(x =>
                {
                    var success = _ackDetecRegExp.Match(x.Payload).Success;
                    return success;
                }).
                    Select(x => x).Subscribe(onNext, onError, onCompleted);
            }
            else
            {
                _logger.LogInformation("Ack Sink disabled. Bypassing subscription to Kafka message stream");
            }
        }

        private void onCompleted()
        {
            _logger.LogInformation("stream has completed.");
        }

        private void onError(Exception ex)
        {
            _logger.LogError(ex.ToString());
        }

        private void onNext(KafkaMessage km)
        {
            var extractVoltageRegExp = new Regex("[0-9]{4,5}");

            float voltage = 0.0f;
            try
            {
                var stationFragment = _ackDetecRegExp.Match(km.Payload).Groups[0].Value;

                var match = _stationExtractorRegExp.Match(stationFragment);

                if (match.Success)
                {
                    var intermediateValue = _ackVoltageRegExp.Match(km.Payload).Groups[0].Value;

                    float.TryParse(extractVoltageRegExp.Match(intermediateValue).Groups[0].Value, out voltage);

                    voltage = voltage / 1000.0f;

                    var stationId = match.Groups[0].Value;

                    var station = _stationConfig.Stations.Where(x => x.Id == stationId).FirstOrDefault();

                    _influxClient.WritePirVoltage(km.Id, station.Id, voltage, _timeProvider.GetCurrentTimeUtc());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}