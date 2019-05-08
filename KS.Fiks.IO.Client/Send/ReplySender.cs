using System;
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

        private readonly Action _ack;

        public ReplySender(ISendHandler sendHandler, ReceivedMessage receivedMessage, Action ack)
        {
            _sendHandler = sendHandler;
            _receivedMessage = receivedMessage;
            _ack = ack;
        }

        public async Task<SentMessage> Reply(string messageType, IList<IPayload> payloads)
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

        public void Ack()
        {
            _ack.Invoke();
        }

        private async Task<SentMessage> Reply(string messageType, IPayload payload)
        {
            return await Reply(messageType, new List<IPayload> {payload}).ConfigureAwait(false);
        }

        private MessageRequest CreateMessageRequest(string messageType)
        {
            return new MessageRequest(
                _receivedMessage.SenderAccountId,
                _receivedMessage.ReceiverAccountId,
                messageType,
                null,
                _receivedMessage.MessageId);
        }
    }
}