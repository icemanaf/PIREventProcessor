using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventProcessor.Tests.Unit.Utilities
{
    public static class LoggerHelper
    {
        public static void VerifyLogInfo<T>(this Mock<ILogger<T>> mockLogger,string info)
        {
            mockLogger.Verify(x => x.Log(
          LogLevel.Information,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => true),
          It.IsAny<Exception>(),
          (e,s)=>info));
        }
    }
}
