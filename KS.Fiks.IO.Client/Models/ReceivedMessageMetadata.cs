using System;

namespace KS.Fiks.IO.Client.Models
{
    public class ReceivedMessageMetadata : MessageBase
    {
        public ReceivedMessageMetadata()
        {
        }

        public ReceivedMessageMetadata(ReceivedMessageMetadata metadata)
            : base(metadata)
        {
            SvarPaMelding = metadata.SvarPaMelding;
        }

        public Guid? SvarPaMelding { get; set; }
    }
}