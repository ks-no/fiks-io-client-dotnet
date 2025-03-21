using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Crypto.Models;

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

        public async Task<SendtMelding> Svar(string meldingType, IList<IPayload> payloads, Guid? klientMeldingId = default, CancellationToken cancellationToken = default)
        {
            return await _sendHandler.Send(CreateMessageRequest(meldingType, klientMeldingId), payloads, cancellationToken).ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, Stream melding, string filnavn, Guid? klientMeldingId = default, CancellationToken cancellationToken = default)
        {
            return await Reply(meldingType, new StreamPayload(melding, filnavn), klientMeldingId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, string melding, string filnavn, Guid? klientMeldingId = default, CancellationToken cancellationToken = default)
        {
            return await Reply(meldingType, new StringPayload(melding, filnavn), klientMeldingId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, string filLokasjon, Guid? klientMeldingId = default, CancellationToken cancellationToken = default)
        {
            return await Reply(meldingType, new FilePayload(filLokasjon), klientMeldingId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<SendtMelding> Svar(string meldingType, Guid? klientMeldingId = default, CancellationToken cancellationToken = default)
        {
            return await Svar(meldingType, new List<IPayload>(), klientMeldingId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task AckAsync()
        {
            await _amqpAcknowledgeManager.AckAsync().ConfigureAwait(false);
        }

        public async Task NackAsync()
        {
            await _amqpAcknowledgeManager.NackAsync().ConfigureAwait(false);
        }

        public async Task NackWithRequeueAsync()
        {
            await _amqpAcknowledgeManager.NackWithRequeueAsync().ConfigureAwait(false);
        }

        private async Task<SendtMelding> Reply(string messageType, IPayload payload, Guid? klientMeldingId, CancellationToken cancellationToken)
        {
            return await Svar(messageType, new List<IPayload> {payload}, klientMeldingId).ConfigureAwait(false);
        }

        private MeldingRequest CreateMessageRequest(string messageType, Guid? klientMeldingId)
        {
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