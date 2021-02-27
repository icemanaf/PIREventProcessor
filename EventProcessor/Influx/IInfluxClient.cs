using System;

namespace EventProcessor.Influx
{
    public interface IInfluxClient
    {
        void WritePirDetectEvent(string correlationId, string deviceId, string area, DateTime time);

        void WritePirVoltage(string correlationId, string deviceId, float voltage, DateTime time);
    }
}