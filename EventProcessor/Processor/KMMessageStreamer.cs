using Proto.Models;
using System;

namespace EventProcessor.Processor
{
    public class KMMessageStreamer : IMessageStreamer<KafkaMessage>
    {
        public IObservable<KafkaMessage> MessageSource { get; }

        public KMMessageStreamer(App app)
        {
            MessageSource = app.KafkaMessageStream;
        }
    }
}