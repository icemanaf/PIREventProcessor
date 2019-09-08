using System;
using System.Collections.Generic;
using System.Text;
using Proto.Models;
using PIREventProcessor.MessageActionFilters;

namespace PIREventProcessor.Processor
{
    public interface IMessageProcessor
    {
        void ProcessMessages(KafkaMessage km);

        void AddMessageFilter(IMessageActionFilter messageFilter);

        void RemoveAllMessageFilters();
    }
}
