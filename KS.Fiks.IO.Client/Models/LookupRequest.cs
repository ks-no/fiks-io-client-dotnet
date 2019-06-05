namespace KS.Fiks.IO.Client.Models
{
    public class LookupRequest
    {
        public LookupRequest(string identifikator, string meldingsprotokoll, int sikkerhetsniva)
        {
            Identifikator = identifikator;
            Meldingsprotokoll = meldingsprotokoll;
            Sikkerhetsniva = sikkerhetsniva;
        }

        public string Identifikator { get; }

        public string Meldingsprotokoll { get; }

        public int Sikkerhetsniva { get; }
    }
}