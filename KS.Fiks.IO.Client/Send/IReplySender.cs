using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Send
{
    public interface IReplySender
    {
        Task<SentMessage> Reply(string messageType, IEnumerable<IPayload> payloads);
        
        Task<SentMessage> Reply(string messageType, Stream message, string filename);

        Task<SentMessage> Reply(string messageType, string message, string filename);

        Task<SentMessage> Reply(string messageType, string filePath);

        Task<SentMessage> Reply(string messageType);

        void Ack();
    }
}

/*
 
 public SendtMelding svar(String meldingType, InputStream melding, String filnavn) {
        return svar(meldingType, singletonList(new StreamPayload(melding, filnavn)));
    }

    public SendtMelding svar(String meldingType, String melding, String filnavn) {
        return svar(meldingType, singletonList(new StringPayload(melding, filnavn)));
    }

    public SendtMelding svar(String meldingType, Path melding) {
        return svar(meldingType, singletonList(new FilePayload(melding)));
    }

    public SendtMelding svar(String meldingType) {
        return svar(meldingType, Collections.emptyList());
    }

    public void ack() {
        doQueueAck.run();
    }

*/