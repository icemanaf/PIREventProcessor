using System;
using System.Collections.Generic;
using System.Text;
using Proto.Models;
using EventProcessor.MessageActionFilters;

namespace EventProcessor.Processor
{
    public interface IMessageProcessor
    {
        void ProcessMessages(KafkaMessage km);

        void AddMessageFilter(IMessageActionFilter messageFilter);

        void RemoveAllMessageFilters();
    }
}
