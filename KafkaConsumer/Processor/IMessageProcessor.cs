using System;
using System.Collections.Generic;
using System.Text;
using Proto.Models;

namespace PIREventProcessor.Processor
{
    public interface IMessageProcessor
    {
        void ProcessMessages(KafkaMessage km);
    }
}
