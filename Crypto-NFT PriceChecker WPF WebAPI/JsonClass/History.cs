using System;

namespace Crypto_NFT_PriceChecker_WPF_WebAPI.JsonClass
{
    internal class History
    {
        public DateTime timestamp { get; set; }
        public double price { get; set; }
        public double volume_24h { get; set; }
        public double market_cap { get; set; }
    }
}
