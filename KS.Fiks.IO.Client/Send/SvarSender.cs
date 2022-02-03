using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
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

        public async Task<SendtMelding> Svar(string meldingType, IList<IPayload> payloads, Guid klientMeldingId = default)
        {
            return await _sendHandler.Send(CreateMessageRequest(meldingType, klientMeldingId), payloads).ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, Stream melding, string filnavn, Guid klientMeldingId = default)
        {
            return await Reply(meldingType, new StreamPayload(melding, filnavn), klientMeldingId)
                .ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, string melding, string filnavn, Guid klientMeldingId = default)
        {
            return await Reply(meldingType, new StringPayload(melding, filnavn), klientMeldingId)
                .ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, string filLokasjon, Guid klientMeldingId = default)
        {
            return await Reply(meldingType, new FilePayload(filLokasjon), klientMeldingId)
                .ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, Guid klientMeldingId = default)
        {
            return await Svar(meldingType, new List<IPayload>(), klientMeldingId)
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

        private async Task<SendtMelding> Reply(string messageType, IPayload payload, Guid klientMeldingId)
        {
            return await Svar(messageType, new List<IPayload> {payload}, klientMeldingId).ConfigureAwait(false);
        }

        private MeldingRequest CreateMessageRequest(string messageType, Guid klientMeldingId)
        {
            if (klientMeldingId == Guid.Empty)
            {
                klientMeldingId = Guid.NewGuid();
            }

            return new MeldingRequest(
                avsenderKontoId: _mottattMelding.MottakerKontoId,
                mottakerKontoId: _mottattMelding.AvsenderKontoId,
                klientMeldingId: klientMeldingId,
                meldingType: messageType,
                ttl: null,
                headere: null,
                svarPaMelding: _mottattMelding.MeldingId);
        }
    }
}