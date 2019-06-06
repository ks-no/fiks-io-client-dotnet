using KS.Fiks.IO.Client.Send;

namespace KS.Fiks.IO.Client.Models
{
    public class MottattMeldingArgs
    {
        public MottattMeldingArgs(IMottattMelding melding, ISvarSender sender)
        {
            Melding = melding;
            SvarSender = sender;
        }

        public IMottattMelding Melding { get; }

        public ISvarSender SvarSender { get; }
    }
}