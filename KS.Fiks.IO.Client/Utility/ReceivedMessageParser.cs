using System;
using System.Collections.Generic;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Utility
{
    internal static class ReceivedMessageParser
    {
        private const string SenderAccountIdHeaderName = "avsender-id";

        private const string MessageIdHeaderName = "melding-id";

        private const string AvsenderNavnHeaderName = "avsender-navn";

        private const string MessageTypeHeaderName = "type";

        private const string DokumentlagerIdHeaderName = "dokumentlager-id";

        private const string SvarPaMeldingHeaderName = "svar-til";

        public static ReceivedMessageMetadata Parse(
            string routingKey,
            IBasicProperties properties)
        {
            var headers = properties?.Headers;

            if (headers == null)
            {
                throw new FiksIOMissingHeaderException($"Header is null. Cannot parse header.");
            }

            foreach (var key in headers.Keys)
            {
                Console.WriteLine($"Headers[{key}]: {System.Text.Encoding.UTF8.GetString((byte[]) headers[key])}");
            }

            return new ReceivedMessageMetadata
            {
                MessageId = RequireGuidFromHeader(headers, MessageIdHeaderName),
                MessageType = RequireStringFromHeader(headers, MessageTypeHeaderName),
                ReceiverAccountId = ParseGuid(routingKey, "routingKey"),
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

            return System.Text.Encoding.UTF8.GetString((byte[]) header[headerName]);
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

        private static TimeSpan? ParseTimeSpan(string longAsString, string headerName)
        {
            if (long.TryParse(longAsString, out var longValue))
            {
                return TimeSpan.FromMilliseconds(longValue);
            }
            else
            {
                return null;
            }
        }
    }
}

/*
  public static MottattMeldingMetadata parse(@NonNull Envelope envelope, @NonNull AMQP.BasicProperties properties) {
        return MottattMeldingMetadata.builder()
                .meldingId(requireUUIDFromHeader(properties.getHeaders(), SvarInn2Headers.MELDING_ID))
                .meldingType(requireStringFromHeader(properties.getHeaders(), SvarInn2Headers.MELDING_TYPE))
                .avsenderKontoId(requireUUIDFromHeader(properties.getHeaders(), SvarInn2Headers.AVSENDER_ID))
                .mottakerKontoId(UUID.fromString(envelope.getRoutingKey()))
                .svarPaMelding(getUUIDFromHeader(properties.getHeaders(), SvarInn2Headers.SVAR_PA_MELDING_ID).getOrElse(() -> null))
                .deliveryTag(envelope.getDeliveryTag())
                .ttl(Long.valueOf(properties.getExpiration()))
                .build();
    }
    
    public static final String AVSENDER_ID = "avsender-id";
    public static final String MELDING_ID = "melding-id";
    public static final String AVSENDER_NAVN = "avsender-navn";
    public static final String MELDING_TYPE = "type";
    public static final String DOKUMENTLAGER_ID = "dokumentlager-id";
    public static final String SVAR_PA_MELDING_ID = "svar-til";
    */