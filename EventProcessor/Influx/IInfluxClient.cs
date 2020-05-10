using System;

namespace EventProcessor.Influx
{
    public interface IInfluxClient
    {
        void WritePirDetectEvent(string correlationId, string deviceId, string area, DateTime time);
    }
}