namespace KS.Fiks.IO.Client.Models
{
    public class LookupRequest
    {
        public string Identifier { get; set; }

        public string MessageType { get; set; }

        public int AccessLevel { get; set; }
    }
}