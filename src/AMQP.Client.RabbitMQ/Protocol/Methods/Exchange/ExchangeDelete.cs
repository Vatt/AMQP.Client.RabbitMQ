namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public readonly struct ExchangeDelete
    {
        public readonly string Name;
        public readonly bool IfUnused;
        public readonly bool NoWait;
        internal ExchangeDelete(string name, bool ifUnused, bool nowait = false)
        {
            Name = name;
            IfUnused = ifUnused;
            NoWait = nowait;
        }
        public static ExchangeDelete Create(string name, bool ifUnused = false) => new ExchangeDelete(name, ifUnused);
        public static ExchangeDelete CreateNoWait(string name, bool ifUnused = false) => new ExchangeDelete(name, ifUnused, true);
    }
}
