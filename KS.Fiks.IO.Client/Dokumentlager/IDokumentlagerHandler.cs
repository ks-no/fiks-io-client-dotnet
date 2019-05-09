using System;
using System.IO;

namespace KS.Fiks.IO.Client.Dokumentlager
{
    internal interface IDokumentlagerHandler
    {
        Stream Download(Guid messageId);
    }
}