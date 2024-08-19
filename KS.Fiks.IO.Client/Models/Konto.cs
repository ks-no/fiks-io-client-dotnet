using System;

namespace KS.Fiks.IO.Client.Models
{
    public class Konto
    {
        public Guid FiksOrgId { get; set; }

        public string FiksOrgNavn { get; set; }

        public string Organisasjonsnummer { get; set; }

        public string Kommunenummer { get; set; }

        public Guid KontoId { get; set; }

        public string KontoNavn { get; set; }

        public bool IsGyldigAvsender { get; set; }

        public bool IsGyldigMottaker { get; set; }

        public long AntallKonsumenter { get; set; }

        internal static Konto FromKatalogModel(KatalogKonto katalogKonto)
        {
            return new Konto
            {
                FiksOrgId = katalogKonto.FiksOrgId,
                FiksOrgNavn = katalogKonto.FiksOrgNavn,
                Organisasjonsnummer = katalogKonto.Organisasjonsnummer,
                Kommunenummer = katalogKonto.Kommunenummer,
                KontoId = katalogKonto.KontoId,
                KontoNavn = katalogKonto.KontoNavn,
                IsGyldigAvsender = katalogKonto.Status.GyldigAvsender,
                IsGyldigMottaker = katalogKonto.Status.GyldigMottaker,
                AntallKonsumenter = katalogKonto.Status.AntallKonsumenter
            };
        }
    }
}