using System;
using System.Collections.Generic;
using System.Text;
using Proto.Models;

namespace EventProcessor.MessageActionFilters
{
    public interface IMessageActionFilter
    {
        void Execute(KafkaMessage km);

        bool IsEnabled();
    }
}
