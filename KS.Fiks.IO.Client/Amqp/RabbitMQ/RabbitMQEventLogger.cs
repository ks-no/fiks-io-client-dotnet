using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Logging;

namespace KS.Fiks.IO.Client.Amqp.RabbitMQ
{
    public class RabbitMQEventLogger : EventListener
    {
        private const string EventSourceName = "rabbitmq-dotnet-client";
        private static ILogger<RabbitMQEventLogger> _logger;
        private readonly EventLevel _eventLevel;

        public RabbitMQEventLogger(ILoggerFactory loggerFactory, EventLevel eventLevel)
        {
            _eventLevel = eventLevel;
            _logger = loggerFactory.CreateLogger<RabbitMQEventLogger>();
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);
            if (eventSource.Name == EventSourceName)
            {
                EnableEvents(eventSource, _eventLevel);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var message = $"EventLog from {eventData.EventSource} " +
                          $", eventData.Level: {eventData.Level}, eventData.Message: {eventData.Message}";

            var i = 0;
            foreach (var payload in eventData.Payload)
            {
                if (payload is string)
                {
                    message += $", Message: {payload}";
                }
                else
                {
                    try
                    {
                        var payloadAsDictionary = ConvertObject<Dictionary<string, object>>(payload);
                        if (payloadAsDictionary != null)
                        {
                            var rabbitMqExceptionDetail = new RabbitMqExceptionDetail(payloadAsDictionary);
                            message += $", RabbitMqExceptionDetail message: {rabbitMqExceptionDetail.Message}, RabbitMqExceptionDetail stacktrace: {rabbitMqExceptionDetail.StackTrace}, RabbitMqExceptionDetail inner exception: {rabbitMqExceptionDetail.InnerException}, RabbitMqExceptionDetail type: {rabbitMqExceptionDetail.Type}";
                        }
                    }
                    catch (Exception e)
                    {
                        //Do nothing
                    }
                }

                i++;
            }

            if (eventData.Level.ToString().ToLower().Contains("err"))
            {
                _logger.LogError(message);
            } else if (eventData.Level.ToString().ToLower().Contains("warn"))
            {
                _logger.LogWarning(message);
            }
            else
            {
                _logger.LogInformation(message);
            }
        }

        private static T ConvertObject<T>(object m)
            where T : class
        {
            var obj = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(m));
            return obj;
        }
    }
}