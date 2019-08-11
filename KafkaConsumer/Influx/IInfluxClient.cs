using System;

namespace PIREventProcessor.Influx
{
    public interface IInfluxClient
    {
        void WritePirDetectEvent(string correlationId, string deviceId, string area, DateTime time);
    }
}