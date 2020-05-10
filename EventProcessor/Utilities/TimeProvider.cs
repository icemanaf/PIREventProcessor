using System;

namespace EventProcessor.Utilities
{
    public class TimeProvider : ITimeProvider
    {
        public DateTime GetCurrentTimeUtc()
        {
            return DateTime.UtcNow;
        }
    }
}