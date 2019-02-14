namespace Ks.Fiks.Svarinn.Client.Maskinporten
{
    public struct MaskinportenClientProperties
    {
        public string Audience { get; private set; }
        public string TokenEndpoint{ get; private set; }
        public string Issuer{ get; private set; }
        public int NumberOfSecondsLeftBeforeExpire{ get; private set; }

        public MaskinportenClientProperties(string audience, string tokenEndpoint, string issuer,
            int numberOfSecondsLeftBeforeExpire)
        {
            Audience = audience;
            TokenEndpoint = tokenEndpoint;
            Issuer = issuer;
            NumberOfSecondsLeftBeforeExpire = numberOfSecondsLeftBeforeExpire;
        }
    }
}