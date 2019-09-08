using Microsoft.Extensions.Logging;
using PIREventProcessor.MessageActionFilters;
using Proto.Models;
using System;
using System.Collections.Generic;

namespace PIREventProcessor.Processor
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly ILogger _ilogger;

        private List<IMessageActionFilter> _messageFilters = new List<IMessageActionFilter>();

        public MessageProcessor(ILogger<MessageProcessor> logger)
        {
            _ilogger = logger;
        }

        public void AddMessageFilter(IMessageActionFilter messageFilter)
        {
            _messageFilters.Add(messageFilter);
        }

        public void ProcessMessages(KafkaMessage km)
        {
            try
            {
                foreach (var filter in _messageFilters)
                {
                    filter.Execute(km);
                }
            }
            catch (Exception e)
            {
                _ilogger.LogError(e.ToString());
            }
        }

        public void RemoveAllMessageFilters()
        {
            _messageFilters.Clear();
        }
    }
}