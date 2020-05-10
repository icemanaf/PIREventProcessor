using System;

namespace EventProcessor.Utilities
{
    public interface ITimeProvider
    {
        DateTime GetCurrentTimeUtc();
    }
}