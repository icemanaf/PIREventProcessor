using Proto.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventProcessor.MessageActionFilters.PIR
{
    public class AckFilter
    {
        public AckFilter()
        {

        }

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
