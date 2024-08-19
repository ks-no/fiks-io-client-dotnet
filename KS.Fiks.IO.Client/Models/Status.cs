using System;

namespace KS.Fiks.IO.Client.Models
{
    public class Status
    {
        public bool IsGyldigAvsender { get; set; }

        public bool IsGyldigMottaker { get; set; }

        public long AntallKonsumenter { get; set; }

        public string Melding { get; set; }

        internal static Status FromKatalogModel(KontoSvarStatus kontoSvarStatus)
        {
            return new Status
            {
                IsGyldigAvsender = kontoSvarStatus.GyldigAvsender,
                IsGyldigMottaker = kontoSvarStatus.GyldigMottaker,
                AntallKonsumenter = kontoSvarStatus.AntallKonsumenter,
                Melding = kontoSvarStatus.Melding
            };
        }
    }
}