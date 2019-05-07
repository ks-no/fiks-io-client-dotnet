using System;
using System.Collections.Generic;
using System.Globalization;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Utility
{
    internal static class ReceivedMessageParser
    {
        private const string SenderAccountIdHeaderName = "avsender-id";

        private const string MessageIdHeaderName = "melding-id";

        private const string MessageTypeHeaderName = "type";

        private const string SvarPaMeldingHeaderName = "svar-til";

        public static ReceivedMessageMetadata Parse(
            Guid receiverAccountId,
            IBasicProperties properties)
        {
            var headers = properties?.Headers;

            if (headers == null)
            {
                throw new FiksIOMissingHeaderException($"Header is null. Cannot parse header.");
            }

            return new ReceivedMessageMetadata
            {
                MessageId = RequireGuidFromHeader(headers, MessageIdHeaderName),
                MessageType = RequireStringFromHeader(headers, MessageTypeHeaderName),
                ReceiverAccountId = receiverAccountId,
                SenderAccountId = RequireGuidFromHeader(headers, SenderAccountIdHeaderName),
                SvarPaMelding = GetGuidFromHeader(headers, SvarPaMeldingHeaderName),
                Ttl = ParseTimeSpan(properties.Expiration, "Ttl")
            };
        }

        private static Guid? GetGuidFromHeader(IDictionary<string, object> header, string headerName)
        {
            try
            {
                return RequireGuidFromHeader(header, headerName);
            }
            catch (Exception ex) when (ex is FiksIOParseException || ex is FiksIOMissingHeaderException)
            {
                return null;
            }
        }

        private static Guid RequireGuidFromHeader(IDictionary<string, object> header, string headerName)
        {
            var headerAsString = RequireStringFromHeader(header, headerName);
            return ParseGuid(headerAsString, headerName);
        }

        private static string RequireStringFromHeader(IDictionary<string, object> header, string headerName)
        {
            if (!header.ContainsKey(headerName))
            {
                throw new FiksIOMissingHeaderException($"Could not find required header: {headerName}.");
            }

            return System.Text.Encoding.UTF8.GetString((byte[])header[headerName]);
        }

        private static Guid ParseGuid(string guidAsString, string headerName)
        {
            if (Guid.TryParse(guidAsString, out var headerValue))
            {
                return headerValue;
            }
            else
            {
                throw new FiksIOParseException(
                    $"Unable to convert header ({headerName}) from string ({guidAsString}) to Guid");
            }
        }

        private static TimeSpan ParseTimeSpan(string longAsString, string headerName)
        {
            if (long.TryParse(longAsString, NumberStyles.Any, CultureInfo.InvariantCulture, out var timeAsLong))
            {
                return TimeSpan.FromMilliseconds(timeAsLong);
            }
            else
            {
                throw new FiksIOParseException(
                    $"Unable to convert header ({headerName}) from string ({longAsString}) to long");
            }
        }
    }
}