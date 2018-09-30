using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitSemaphore.Core
{

    public class ResourceManager : IDisposable
    {
        private readonly string _clientId;
        private readonly Action<string> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public ResourceManager(string clientId, Action<string> logger = null)
        {
            _clientId = clientId;
            _logger = logger ?? Console.WriteLine;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = ConfigurationManager.AppSettings["RabbitUserName"],
                Password = ConfigurationManager.AppSettings["RabbitPassword"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        /// <summary>
        /// Should only be called only once (ever) for a given resource
        /// </summary>
        public void ProtectResource(string protectedResource)
        {
            var queueName = QueueName(protectedResource);
            
            //Make sure queue is empty before adding lock
            _channel.QueueDelete(queueName, false, false);
            DeclareQueue(queueName);

            _logger($"Protecting Resource \"{protectedResource}\"");
          
            var body = Encoding.UTF8.GetBytes("lock");
            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: body);
        }

        private QueueDeclareOk DeclareQueue(string queueName)
        {
            var result = _channel.QueueDeclare(queueName, true, false, false, null);
            return result;
        }

        public void ExecuteTask(string protectedResource, Action action)
        {
            var queueName = QueueName(protectedResource);
            DeclareQueue(queueName);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                action();

                //when action is done send lock back to Rabbit
                _channel.BasicReject(ea.DeliveryTag, true);
            };

            _logger($"{_clientId} consuming {protectedResource}...");

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
        }


        private static string QueueName(string protectedResource)
        {
            return $"semaphore.{protectedResource}";
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }

}