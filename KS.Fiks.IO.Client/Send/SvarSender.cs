using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Send
{
    public class SvarSender : ISvarSender
    {
        private readonly ISendHandler _sendHandler;

        private readonly MottattMelding _mottattMelding;

        private readonly IAmqpAcknowledgeManager _amqpAcknowledgeManager;

        public SvarSender(ISendHandler sendHandler, MottattMelding mottattMelding, IAmqpAcknowledgeManager amqpAcknowledgeManager)
        {
            _sendHandler = sendHandler;
            _mottattMelding = mottattMelding;
            _amqpAcknowledgeManager = amqpAcknowledgeManager;
        }

        public async Task<SendtMelding> Svar(string meldingType, IList<IPayload> payloads)
        {
            return await _sendHandler.Send(CreateMessageRequest(meldingType), payloads).ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, Stream melding, string filnavn)
        {
            return await Reply(meldingType, new StreamPayload(melding, filnavn))
                .ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, string melding, string filnavn)
        {
            return await Reply(meldingType, new StringPayload(melding, filnavn))
                .ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, string filLokasjon)
        {
            return await Reply(meldingType, new FilePayload(filLokasjon))
                .ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType)
        {
            return await Svar(meldingType, new List<IPayload>())
                .ConfigureAwait(false);
        }

        public void Ack()
        {
            this._amqpAcknowledgeManager.Ack().Invoke();
        }

        public void Nack()
        {
            this._amqpAcknowledgeManager.Nack().Invoke();
        }

        public void NackWithRequeue()
        {
            this._amqpAcknowledgeManager.NackWithRequeue().Invoke();
        }

        private async Task<SendtMelding> Reply(string messageType, IPayload payload)
        {
            return await Svar(messageType, new List<IPayload> {payload}).ConfigureAwait(false);
        }

        private MeldingRequest CreateMessageRequest(string messageType)
        {
            return new MeldingRequest(
                avsenderKontoId: _mottattMelding.MottakerKontoId,
                mottakerKontoId: _mottattMelding.AvsenderKontoId,
                meldingType: messageType,
                ttl: null,
                headere: null,
                svarPaMelding: _mottattMelding.MeldingId);
        }
    }
}