using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Connections;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Exceptions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AMQP.Client.RabbitMQ.Tests
{
    public class ConnectionTest : TestBase
    {
        private TaskCompletionSource _connectionCloseTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        public ConnectionTest() : base()
        {
            
        }
        [Fact]
        public async Task ConnectAndClose()
        {
            var factory = RabbitMQConnectionFactory.Create(new DnsEndPoint(Host, 5672), builder =>
            {
                var loggerFactory = LoggerFactory.Create(loggerBuilder =>
                {
                    loggerBuilder.AddConsole();
                });
                builder.AddLogger(loggerFactory.CreateLogger(string.Empty));
                builder.OnConnectionClose((obj, args) =>
                {
                    _connectionCloseTcs.SetResult();
                });
            });
            var connection = factory.CreateConnection();

            await connection.StartAsync();
                
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Seconds));

            using (var timeoutRegistration = cts.Token.Register(() => _connectionCloseTcs.SetCanceled()))
            {
                await connection.CloseAsync();
                await _connectionCloseTcs.Task;
            }
            Assert.True(_connectionCloseTcs.Task.IsCompleted && !_connectionCloseTcs.Task.IsCanceled);
            
            

        }

        [Fact]
        public async Task DoubleConnectionCloseTest()
        {
            var factory = RabbitMQConnectionFactory.Create(new DnsEndPoint(Host, 5672), builder =>
            {
                var loggerFactory = LoggerFactory.Create(loggerBuilder =>
                {
                    loggerBuilder.AddConsole();
                });
                builder.AddLogger(loggerFactory.CreateLogger(string.Empty));
                builder.OnConnectionClose((obj, args) =>
                {
                    _connectionCloseTcs.SetResult();
                });
            });
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Seconds));
            using (var timeoutRegistration = cts.Token.Register(() => _connectionCloseTcs.SetCanceled()))
            {
                await connection.CloseAsync();
                await _connectionCloseTcs.Task;
            }
            Assert.True(_connectionCloseTcs.Task.IsCompleted);

            try
            {
                await connection.CloseAsync();
            }
            catch (ConnectionClosedException ex)
            {
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.True(false);
            }

            try
            {
                var test = await connection.OpenChannel();
            }
            catch (ConnectionClosedException ex)
            {
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.True(false);
            }
            
            try
            {
                var test = await connection.StartAsync();
            }
            catch (ConnectionClosedException ex)
            {
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.True(false);
            }
            Assert.True(connection.Closed);
        }

        [Fact]
        public async Task DoubleStartConnection()
        {
            var factory = RabbitMQConnectionFactory.Create(new DnsEndPoint(Host, 5672), builder =>
            {
                var loggerFactory = LoggerFactory.Create(loggerBuilder =>
                {
                    loggerBuilder.AddConsole();
                });
                builder.AddLogger(loggerFactory.CreateLogger(string.Empty));
                builder.OnConnectionClose((obj, args) =>
                {
                    _connectionCloseTcs.SetResult();
                });
            });
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            try
            {
                await connection.StartAsync();
            }
            catch (InvalidOperationException e)
            {
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.True(false);
            }
        }
    }
}