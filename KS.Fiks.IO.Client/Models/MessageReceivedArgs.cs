using KS.Fiks.IO.Client.Send;

namespace KS.Fiks.IO.Client.Models
{
    public class MessageReceivedArgs
    {
        public MessageReceivedArgs(ReceivedMessage message, IReplySender sender)
        {
            Message = message;
            ReplySender = sender;
        }

        public ReceivedMessage Message { get; }

        public IReplySender ReplySender { get; }
    }
}