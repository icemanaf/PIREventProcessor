using System;

namespace EventProcessor.Processor
{
    public interface IMessageStreamer<T>
    {
        IObservable<T> MessageSource { get; }
    }
}