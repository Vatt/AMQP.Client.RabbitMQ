using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;

namespace AMQP.Client.RabbitMQ
{
    internal static class RabbitMQSessionQueueExt
    {
        public static async ValueTask<QueueDeclareOk> QueueDeclareAsync(this RabbitMQSession session, RabbitMQChannel channel, QueueDeclare queue)
        {
            session.Channels.TryGetValue(channel.ChannelId, out var src);
            var data = session.GetChannelData(channel.ChannelId);
            src.QueueTcs = new TaskCompletionSource<QueueDeclareOk>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.NoWait = false;
            await session.Writer.SendQueueDeclareAsync(channel.ChannelId, queue).ConfigureAwait(false);
            var declare = await src.QueueTcs.Task.ConfigureAwait(false);
            data.Queues.Add(queue.Name, queue);
            return declare;
        }
        public static async ValueTask QueueDeclareNoWaitAsync(this RabbitMQSession session, RabbitMQChannel channel, QueueDeclare queue)
        {
            session.Channels.TryGetValue(channel.ChannelId, out var src);
            var data = session.GetChannelData(channel.ChannelId);
            queue.NoWait = true;
            await session.Writer.SendQueueDeclareAsync(channel.ChannelId, queue).ConfigureAwait(false);
            data.Queues.Add(queue.Name, queue);
        }
        public static async ValueTask<int> QueueDeleteAsync(this RabbitMQSession session, RabbitMQChannel channel, QueueDelete queue)
        {
            session.Channels.TryGetValue(channel.ChannelId, out var src);
            var data = session.GetChannelData(channel.ChannelId);
            src.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.NoWait = false;
            await session.Writer.SendQueueDeleteAsync(channel.ChannelId, queue).ConfigureAwait(false);
            var deleted = await src.CommonTcs.Task.ConfigureAwait(false);
            data.Queues.Remove(queue.Name);
            return deleted;
        }
        public static async ValueTask QueueDeleteNoWaitAsync(this RabbitMQSession session, RabbitMQChannel channel, QueueDelete queue)
        {
            session.Channels.TryGetValue(channel.ChannelId, out var src);
            var data = session.GetChannelData(channel.ChannelId);
            src.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.NoWait = true;
            await session.Writer.SendQueueDeleteAsync(channel.ChannelId, queue).ConfigureAwait(false);
            data.Queues.Remove(queue.Name);
        }
        public static async ValueTask<int> QueuePurgeAsync(this RabbitMQSession session, RabbitMQChannel channel, QueuePurge queue)
        {
            session.Channels.TryGetValue(channel.ChannelId, out var src);
            var data = session.GetChannelData(channel.ChannelId);
            src.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.NoWait = false;
            await session.Writer.SendQueuePurgeAsync(channel.ChannelId, queue).ConfigureAwait(false);
            var deleted = await src.CommonTcs.Task.ConfigureAwait(false);
            data.Queues.Remove(queue.Name);
            return deleted;
        }
        public static async ValueTask QueuePurgeNoWaitAsync(this RabbitMQSession session, RabbitMQChannel channel, QueuePurge queue)
        {
            session.Channels.TryGetValue(channel.ChannelId, out var src);
            var data = session.GetChannelData(channel.ChannelId);
            src.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.NoWait = true;
            await session.Writer.SendQueuePurgeAsync(channel.ChannelId, queue).ConfigureAwait(false);
            data.Queues.Remove(queue.Name);
        }
        public static async ValueTask QueueBindAsync(this RabbitMQSession session, RabbitMQChannel channel, QueueBind bind)
        {
            session.Channels.TryGetValue(channel.ChannelId, out var src);
            var data = session.GetChannelData(channel.ChannelId);
            src.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await session.Writer.SendQueueBindAsync(channel.ChannelId, bind).ConfigureAwait(false);
            if (bind.NoWait)
            {
                data.Binds.Add(bind.QueueName, bind);
                return;
            }
            await src.CommonTcs.Task.ConfigureAwait(false);
            data.Binds.Add(bind.QueueName, bind);
        }

        public static async ValueTask QueueUnbindAsync(this RabbitMQSession session, RabbitMQChannel channel, QueueUnbind unbind)
        {
            session.Channels.TryGetValue(channel.ChannelId, out var src);
            src.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await session.Writer.SendQueueUnbindAsync(channel.ChannelId, unbind).ConfigureAwait(false);
            await src.CommonTcs.Task.ConfigureAwait(false);
        }
    }
}
