using System;

namespace KS.Fiks.IO.Client.Configuration
{
    public class RateLimitConfiguration
    {
        public int BucketSize { get; }

        public TimeSpan TokenRefillInterval { get; }

        public RateLimitConfiguration()
        {
            BucketSize = 5;
            TokenRefillInterval = TimeSpan.FromSeconds(2);
        }

        public RateLimitConfiguration(int bucketSize, TimeSpan tokenRefillInterval)
        {
            BucketSize = bucketSize;
            TokenRefillInterval = tokenRefillInterval;
        }
    }
}