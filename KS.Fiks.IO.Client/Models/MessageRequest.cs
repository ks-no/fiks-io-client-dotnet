using System;
using KS.Fiks.IO.Send.Client;

namespace KS.Fiks.IO.Client.Models
{
    public class MessageRequest
    {
        private const int DefaultTtlInDays = 2;
        
        public MessageRequest()
        {
            Ttl = TimeSpan.FromDays(DefaultTtlInDays);
        }

        public Guid SenderAccountId { get; set; }

        public Guid ReceiverAccountId { get; set; }

        public string MessageType { get; set; }

        public TimeSpan Ttl { get; set; }

        public Guid SvarPaMelding { get; set; }

        public MessageSpecificationApiModel ToApiModel()
        {
            return new MessageSpecificationApiModel
            {
                AvsenderKontoId = SenderAccountId,
                MeldingType = MessageType,
                MottakerKontoId = ReceiverAccountId,
                SvarPaMelding = SvarPaMelding,
                Ttl = (long)Ttl.TotalMilliseconds
            };
        }
    }
}