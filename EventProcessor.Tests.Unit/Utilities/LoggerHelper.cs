using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace EventProcessor.Tests.Unit.Utilities
{
    public static class LoggerHelper
    {
        public static Mock<ILogger<T>> VerifyLogInfo<T>(this Mock<ILogger<T>> mockLogger, string info)
        {
            mockLogger.Verify(x => x.Log(
          LogLevel.Information,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(info)),
          It.IsAny<Exception>(),
          It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            return mockLogger;
        }
    }
}