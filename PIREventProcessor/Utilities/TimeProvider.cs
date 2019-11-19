using System;

namespace PIREventProcessor.Utilities
{
    public class TimeProvider : ITimeProvider
    {
        public DateTime GetCurrentTimeUtc()
        {
            return DateTime.UtcNow;
        }
    }
}