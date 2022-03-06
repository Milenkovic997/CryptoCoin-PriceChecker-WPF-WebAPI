using System;

namespace Crypto_NFT_PriceChecker_WPF_WebAPI.CryptoDescription5m
{
    internal class CryptoDescription
    {
        public string name { get; set; }
        public string symbol { get; set; }
        public string price { get; set; }
        public double marketCap { get; set; }
        public double hVolume { get; set; }
        public DateTime timestamp { get; set; }
        public string hpercent { get; set; }
    }
}
