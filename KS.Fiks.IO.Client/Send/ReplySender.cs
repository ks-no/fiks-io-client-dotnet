using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Send
{
    public class ReplySender : IReplySender
    {
        private readonly ISendHandler _sendHandler;

        private readonly ReceivedMessage _receivedMessage;

        public ReplySender(ISendHandler sendHandler, ReceivedMessage receivedMessage)
        {
            _sendHandler = sendHandler;
            _receivedMessage = receivedMessage;
        }

        public async Task<SentMessage> Reply(string messageType, IEnumerable<IPayload> payloads)
        {
            return await _sendHandler.Send(CreateMessageRequest(messageType), payloads).ConfigureAwait(false);
        }

        public async Task<SentMessage> Reply(string messageType, Stream message, string filename)
        {
            return await Reply(messageType, new StreamPayload(message, filename))
                .ConfigureAwait(false);
        }

        public async Task<SentMessage> Reply(string messageType, string message, string filename)
        {
            return await Reply(messageType, new StringPayload(message, filename))
                .ConfigureAwait(false);
        }

        public async Task<SentMessage> Reply(string messageType, string filePath)
        {
            return await Reply(messageType, new FilePayload(filePath))
                .ConfigureAwait(false);
        }

        public async Task<SentMessage> Reply(string messageType)
        {
            return await Reply(messageType, new List<IPayload>())
                .ConfigureAwait(false);
        }

        private async Task<SentMessage> Reply(string messageType, IPayload payload)
        {
            return await Reply(messageType, new List<IPayload> {payload}).ConfigureAwait(false);
        }

        public void Ack()
        {
            throw new System.NotImplementedException();
        }

        private MessageRequest CreateMessageRequest(string messageType)
        {
            return new MessageRequest
            {
                MessageType = messageType,
                ReceiverAccountId = _receivedMessage.SenderAccountId,
                SenderAccountId = _receivedMessage.ReceiverAccountId,
                SvarPaMelding = _receivedMessage.MessageId
            };
        }
    }
}