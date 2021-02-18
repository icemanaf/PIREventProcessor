using Proto.Models;
using System;

namespace EventProcessor.MessageActionFilters.PIR
{
    public class AckFilter : IEventSink<KafkaMessage>
    {
        public AckFilter()
        {
        }

        public bool Enabled()
        {
            return true;
        }

        public void Observe(IObservable<KafkaMessage> observable)
        {
            //throw new NotImplementedException();
        }
    }
}