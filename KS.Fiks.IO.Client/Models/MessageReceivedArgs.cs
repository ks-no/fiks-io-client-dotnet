namespace KS.Fiks.IO.Client.Models
{
    public class MessageReceivedArgs
    {
        public MessageReceivedArgs(ReceivedMessage message, IResponseSender sender)
        {
            Message = message;
            ResponseSender = sender;
        }

        public ReceivedMessage Message { get; }

        public IResponseSender ResponseSender { get; }
    }
}