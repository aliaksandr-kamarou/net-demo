namespace EMarketApi.Infrastructure.Cache
{
    public class RedisOptions
    {
        public string Host { get; set; }

        public string Port { get; set; }

        public int DefaultExpiration { get; set; }

        public string ConnectionString => $"{this.Host}:{this.Port}";
    }
}
