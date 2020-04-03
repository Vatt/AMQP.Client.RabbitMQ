using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    internal static class ChannelHandlerQueueExt
    {
        public static async ValueTask<QueueDeclareOk> QueueDeclareAsync(this ChannelHandler handler, RabbitMQChannel channel, QueueDeclare queue)
        {
            handler.Channels.TryGetValue(channel.ChannelId, out var data);
            data.QueueTcs = new TaskCompletionSource<QueueDeclareOk>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.NoWait = false;
            await handler.Protocol.SendQueueDeclareAsync(channel.ChannelId, queue).ConfigureAwait(false);
            var declare = await data.QueueTcs.Task.ConfigureAwait(false);
            data.Queues.Add(queue.Name, queue);
            return declare;
        }
        public static async ValueTask QueueDeclareNoWaitAsync(this ChannelHandler handler, RabbitMQChannel channel, QueueDeclare queue)
        {
            handler.Channels.TryGetValue(channel.ChannelId, out var data);
            queue.NoWait = true;
            await handler.Protocol.SendQueueDeclareAsync(channel.ChannelId, queue).ConfigureAwait(false);
            data.Queues.Add(queue.Name, queue);
        }
        public static async ValueTask<int> QueueDeleteAsync(this ChannelHandler handler, RabbitMQChannel channel, QueueDelete queue)
        {
            handler.Channels.TryGetValue(channel.ChannelId, out var data);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.NoWait = false;
            await handler.Protocol.SendQueueDeleteAsync(channel.ChannelId, queue).ConfigureAwait(false);
            var deleted = await data.CommonTcs.Task.ConfigureAwait(false);
            data.Queues.Remove(queue.Name);
            return deleted;
        }
        public static async ValueTask QueueDeleteNoWaitAsync(this ChannelHandler handler, RabbitMQChannel channel, QueueDelete queue)
        {
            handler.Channels.TryGetValue(channel.ChannelId, out var data);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.NoWait = true;
            await handler.Protocol.SendQueueDeleteAsync(channel.ChannelId, queue).ConfigureAwait(false);
            data.Queues.Remove(queue.Name);
        }
        public static async ValueTask<int> QueuePurgeAsync(this ChannelHandler handler, RabbitMQChannel channel, QueuePurge queue)
        {
            handler.Channels.TryGetValue(channel.ChannelId, out var data);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.NoWait = false;
            await handler.Protocol.SendQueuePurgeAsync(channel.ChannelId, queue).ConfigureAwait(false);
            var deleted = await data.CommonTcs.Task.ConfigureAwait(false);
            data.Queues.Remove(queue.Name);
            return deleted;
        }
        public static async ValueTask QueuePurgeNoWaitAsync(this ChannelHandler handler, RabbitMQChannel channel, QueuePurge queue)
        {
            handler.Channels.TryGetValue(channel.ChannelId, out var data);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.NoWait = true;
            await handler.Protocol.SendQueuePurgeAsync(channel.ChannelId, queue).ConfigureAwait(false);
            data.Queues.Remove(queue.Name);
        }
        public static async ValueTask QueueBindAsync(this ChannelHandler handler, RabbitMQChannel channel, QueueBind bind)
        {
            handler.Channels.TryGetValue(channel.ChannelId, out var data);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await handler.Protocol.SendQueueBindAsync(channel.ChannelId, bind).ConfigureAwait(false);
            if (bind.NoWait)
            {
                data.Binds.Add(bind.QueueName, bind);
                return;
            }
            await data.CommonTcs.Task.ConfigureAwait(false);
            data.Binds.Add(bind.QueueName, bind);
        }

        public static async ValueTask QueueUnbindAsync(this ChannelHandler handler, RabbitMQChannel channel, QueueUnbind unbind)
        {
            handler.Channels.TryGetValue(channel.ChannelId, out var data);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await handler.Protocol.SendQueueUnbindAsync(channel.ChannelId, unbind).ConfigureAwait(false);
            await data.CommonTcs.Task.ConfigureAwait(false);
        }
    }
}
