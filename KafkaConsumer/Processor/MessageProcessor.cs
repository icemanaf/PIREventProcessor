using System;
using System.Collections.Generic;
using System.Text;
using PIREventProcessor.Influx;
using Proto.Models;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace PIREventProcessor.Processor
{
    public class MessageProcessor:IMessageProcessor
    {
        private readonly IInfluxClient _influxClient;
        private readonly ILogger _ilogger;
        private readonly StationConfig _stationConfig;

        private Regex _pir_detect_regexp = new Regex("A{3,}D{3,}[0-9]{4,6}Z{3,}", RegexOptions.Compiled);

        

        public MessageProcessor(ILogger<MessageProcessor> logger,IInfluxClient influxClient, IOptions<StationConfig> config)
        {
            _ilogger = logger;

            _influxClient = influxClient;

            _stationConfig = config.Value;
        }
        public void ProcessMessages(KafkaMessage km)
        {

           var pir_detection_match = _pir_detect_regexp.Match(km.Payload);

           if (pir_detection_match.Success)
           {
                //just a comment     
           }
        }
    }
}
