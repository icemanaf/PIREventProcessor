using System;
using System.Collections.Generic;
using InfluxDB.Collector;
using Microsoft.Extensions.Options;

namespace PIREventProcessor.Influx
{
    public class InfluxClient : IInfluxClient
    {
        private readonly InfluxConfig _config;

        public InfluxClient(IOptions<InfluxConfig> config)
        {
            _config = config.Value;
        }

        public void WritePirDetectEvent(string correlationId, string deviceId, string area, DateTime time)
        {
            using (Metrics.Collector = new CollectorConfiguration().Batch.AtInterval(TimeSpan.FromSeconds(1)).WriteTo
                .InfluxDB(_config.InfluxServer, _config.Database).CreateCollector())
            {
                Metrics.Write("pir_detection", new Dictionary<string, object>
                {
                    {"correlation_id", correlationId},
                    {"device_id", deviceId},
                    {"area", area},
                    {"detect_time", $"{time:dd-MMM-yyyy HH:ss}"}
                });
            }
        }
    }
}