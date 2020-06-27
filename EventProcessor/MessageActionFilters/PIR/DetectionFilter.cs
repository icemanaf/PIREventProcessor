using Proto.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EventProcessor.MessageActionFilters.PIR
{
    /// <summary>
    /// This filter detects when a PIR Detect message is broadcast by the RF gateway
    /// Example of a signal is AAAADDD4445ZZZZ , the digits in the middle are the sensor identifier which can be mapped to a location
    /// </summary>
    public class DetectionFilter : IMessageActionFilter
    {
        private Regex _pirDetectRegExp = new Regex("A{3,}D{3,}[0-9]{4,6}Z{3,}", RegexOptions.Compiled);

        public void Execute(KafkaMessage km)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled()
        {
            throw new NotImplementedException();
        }
    }
}
