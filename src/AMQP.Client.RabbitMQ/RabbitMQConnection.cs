using Microsoft.AspNetCore.Connections;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Methods;
using Bedrock.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnection
    {
        private static readonly Bedrock.Framework.Client _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider())
                                                                    .UseSockets()
                                                                    .Build();
        
        private readonly object _lockObj = new object();
        private readonly CancellationTokenSource _connectionCloseTokenSource = new CancellationTokenSource();
        private readonly CancellationToken ConnectionClosed;
        private ConnectionContext _context;
        public EndPoint RemoteEndPoint => _context.RemoteEndPoint;
        public IDuplexPipe Transport => _context.Transport;
        private readonly ReaderDispatcher Dispatcher;
        
        public RabbitMQServerInfo ServerInfo { get; private set; }
        public readonly int Chanell;
        public RabbitMQConnection()
        {
            //_context = context;
            //Info = serverInfo;
            Chanell = 1;
            ConnectionClosed = _connectionCloseTokenSource.Token;
            Dispatcher = new ReaderDispatcher();
        }
        private async Task StartReadingAsync()
        {
            while(true)
            {
                
                var result = await Transport.Input.ReadAsync();
                if(result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
                Dispatcher.OnPipeReader(result.Buffer, out SequencePosition position);
                Transport.Input.AdvanceTo(position);


            }           

        }
        public async Task StartAsync(IPEndPoint endpoint)
        {
            _context = await _client.ConnectAsync(endpoint);
            StartMethod start = new StartMethod(Dispatcher, Transport.Output);
            ServerInfo = await start.InvokeAsync();
            await StartReadingAsync();
        }
        public void CloseConnection()
        {
            lock (_lockObj)
            {
                _connectionCloseTokenSource.Cancel();
                _context.Abort();
                _connectionCloseTokenSource.Dispose();
            }

        }
    }
}
