using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Utility
{
    internal static class ReceivedMessageParser
    {
        public const string EgendefinertHeaderPrefix = "egendefinert-header.";

        private const string SenderAccountIdHeaderName = "avsender-id";

        private const string MessageIdHeaderName = "melding-id";

        private const string MessageTypeHeaderName = "type";

        private const string RelatedMessageIdHeaderName = "svar-til";

        private const string TtlHeaderName = "Ttl";

        internal static MottattMeldingMetadata Parse(
            Guid receiverAccountId,
            IReadOnlyBasicProperties properties,
            bool resendt)
        {
            var headers = properties?.Headers;

            if (headers == null)
            {
                throw new FiksIOMissingHeaderException($"Header is null. Cannot parse header.");
            }

            return new MottattMeldingMetadata(
                meldingId: RequireGuidFromHeader(headers, MessageIdHeaderName),
                meldingType: RequireStringFromHeader(headers, MessageTypeHeaderName),
                mottakerKontoId: receiverAccountId,
                avsenderKontoId: RequireGuidFromHeader(headers, SenderAccountIdHeaderName),
                svarPaMelding: GetGuidFromHeader(headers, RelatedMessageIdHeaderName),
                ttl: ParseTimeSpan(properties.Expiration),
                headere: ExtractEgendefinerteHeadere(headers),
                resendt);
        }

        internal static Guid RequireGuidFromHeader(IDictionary<string, object> header, string headerName)
        {
            var headerAsString = RequireStringFromHeader(header, headerName);
            return ParseGuid(headerAsString, headerName);
        }

        internal static Guid? GetGuidFromHeader(IDictionary<string, object> header, string headerName)
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

        private static Dictionary<string, string> ExtractEgendefinerteHeadere(IDictionary<string, object> headers)
        {
            return headers
                .Where(h => h.Key.StartsWith(EgendefinertHeaderPrefix))
                .ToDictionary(
                    h => h.Key.Substring(EgendefinertHeaderPrefix.Length),
                    h => System.Text.Encoding.UTF8.GetString((byte[])h.Value));
        }

        private static string RequireStringFromHeader(IDictionary<string, object> header, string headerName)
        {
            if (!header.ContainsKey(headerName))
            {
                throw new FiksIOMissingHeaderException($"Could not find required header: {headerName}.");
            }

            try
            {
                return System.Text.Encoding.UTF8.GetString((byte[])header[headerName]);
            }
            catch (Exception ex)
            {
                throw new FiksIOParseException($"Unable to parse header({headerName}) from byte[] to string.", ex);
            }
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

        private static TimeSpan ParseTimeSpan(string longAsString)
        {
            return long.TryParse(longAsString, NumberStyles.Any, CultureInfo.InvariantCulture, out var timeAsLong) ? TimeSpan.FromMilliseconds(timeAsLong) : TimeSpan.MaxValue;
        }
    }
}