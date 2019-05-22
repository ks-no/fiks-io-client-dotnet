using KS.Fiks.IO.Client.Send;

namespace KS.Fiks.IO.Client.Models
{
    public class MessageReceivedArgs
    {
        public MessageReceivedArgs(IReceivedMessage message, IReplySender sender)
        {
            Message = message;
            ReplySender = sender;
        }

        public IReceivedMessage Message { get; }

        public IReplySender ReplySender { get; }
    }
}