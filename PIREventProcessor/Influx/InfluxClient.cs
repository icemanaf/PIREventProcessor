using InfluxDB.Collector;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace PIREventProcessor.Influx
{
    public class InfluxClient : IInfluxClient
    {
        private readonly InfluxConfig _config;

        private readonly ILogger<InfluxClient> _ilogger;

        public InfluxClient(IOptions<InfluxConfig> config, ILogger<InfluxClient> logger)
        {
            _config = config.Value;

            _ilogger = logger;
        }

        public void WritePirDetectEvent(string correlationId, string deviceId, string area, DateTime time)
        {
            using (Metrics.Collector = new CollectorConfiguration().Batch.AtInterval(TimeSpan.FromSeconds(1)).WriteTo
                .InfluxDB(_config.InfluxServer, _config.Database).CreateCollector())
            {
                var record = new Dictionary<string, object>
                {
                    {"correlation_id", correlationId},
                    {"device_id", deviceId},
                    {"area", area},
                    {"detect_time", $"{time:dd-MMM-yyyy HH:ss}"}
                };

                Metrics.Write("pir_detection", record);

                _ilogger.LogInformation("updated influxdb with {@m}", record);
            }
        }
    }
}