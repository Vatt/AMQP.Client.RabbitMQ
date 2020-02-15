using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Basic
{
    internal class BasicHandler : BasicReaderWriter
    {
        public BasicHandler(ushort channelId,RabbitMQProtocol protocol):base(channelId,protocol)
        {

        }
        public async ValueTask HandleAsync(FrameHeader header)
        {
            Debug.Assert(header.FrameType == Constants.FrameHeader && header.Channel == _channelId);
            var result = await _protocol.Reader.ReadAsync(new ContentHeaderReader());
            _protocol.Reader.Advance();
            if(result.IsCompleted)
            {
                //TODO: сделать чтонибудь
            }
            ContentHeader contentHeader = result.Message;
        }
        public ValueTask HandleMethodHeader(MethodHeader header)
        {
            Debug.Assert(header.ClassId == 60);
            switch(header.MethodId)
            {
                case 60:
                    {
                        var deliver = ReadBasicDeliver();
                        break;
                    }
            }
            return default;
        }

    }
}
