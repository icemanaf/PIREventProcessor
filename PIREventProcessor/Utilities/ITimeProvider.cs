using System;

namespace PIREventProcessor.Utilities
{
    public interface ITimeProvider
    {
        DateTime GetCurrentTimeUtc();
    }
}