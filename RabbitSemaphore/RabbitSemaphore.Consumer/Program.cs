using System;
using System.Configuration;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitSemaphore.Core;

namespace RabbitSemaphore.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var id = Guid.NewGuid().ToString();
            var randomProtectedResource = ProtectedResourcesExample.All[Random(ProtectedResourcesExample.All.Length - 1)];

            using (var resourceManager = new ResourceManager(id))
            {
                resourceManager.ExecuteTask(randomProtectedResource, () =>
                {
                    Console.WriteLine($"{id} Received lock on {randomProtectedResource}");

                    while (true)
                    {
                        Console.WriteLine($"{id} working on {randomProtectedResource}");
                        Thread.Sleep(5000);
                    }
                });

                Console.ReadKey();
            }
        }


        private static int Random(int max)
        {
            var r = new Random(DateTime.Now.Millisecond);
            return r.Next(0, max);
        }
    }
}
