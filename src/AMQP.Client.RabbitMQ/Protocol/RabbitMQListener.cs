using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQListener
    {
        private IConnectionHandler _connectionHandler;
        CancellationToken _token;
        public async Task StartAsync(RabbitMQProtocolReader reader, IConnectionHandler connection, CancellationToken token = default)
        {
            _connectionHandler = connection;
            _token = token;
            while (true)
            {
                var frame = await reader.ReadAsync(new FrameReader(), token).ConfigureAwait(false);
                if (frame.Header.Channel == 0)
                {
                    await ProcessConnection(reader, frame);
                }
                reader.Advance();
            }
        }
        public async ValueTask ProcessChannel(RabbitMQProtocolReader reader, Frame frame)
        {

        }
        public async ValueTask ProcessConnection(RabbitMQProtocolReader protocol, Frame frame)
        {
            var method = protocol.ReadMethodHeader(frame.Payload);
            var payload = frame.Payload.Slice(4);// MethodHeader size
            switch (method.MethodId)
            {
                case 10:
                    {
                        await _connectionHandler.OnStartAsync(protocol.ReadStart(payload));
                        break;
                    }

                case 30:
                    {
                        await _connectionHandler.OnTuneAsync(protocol.ReadTuneMethod(payload));
                        break;
                    }
                case 41:
                    {
                        var result = protocol.ReadConnectionOpenOk(payload);
                        await _connectionHandler.OnOpenOkAsync();
                        break;
                    }
                case 50: //close
                    {
                        await _connectionHandler.OnCloseAsync(protocol.ReadClose(payload));
                        break;
                    }
                case 51://close-ok
                    {
                        var result = protocol.ReadCloseOk(payload);
                        await _connectionHandler.OnCloseOkAsync();
                        break;
                    }

                default:
                    throw new Exception($"{nameof(RabbitMQListener)}.{nameof(ProcessConnection)} :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
    }
}
