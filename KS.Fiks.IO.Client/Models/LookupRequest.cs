namespace KS.Fiks.IO.Client.Models
{
    public class LookupRequest
    {
        public LookupRequest(string identifier, string messageType, int accessLevel)
        {
            Identifier = identifier;
            MessageType = messageType;
            AccessLevel = accessLevel;
        }

        public string Identifier { get; }

        public string MessageType { get; }

        public int AccessLevel { get; }
    }
}