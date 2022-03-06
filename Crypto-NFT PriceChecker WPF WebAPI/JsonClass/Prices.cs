using System;

namespace Crypto_NFT_PriceChecker_WPF_WebAPI.JsonClass
{
    internal class Prices
    {
        public DateTime time_open { get; set; }
        public DateTime time_close { get; set; }
        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public object volume { get; set; }
        public object market_cap { get; set; }
    }
}
