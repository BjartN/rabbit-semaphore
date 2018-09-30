using System;
using RabbitSemaphore.Consumer;
using RabbitSemaphore.Core;

namespace RabbitSemaphore.StartUp
{
    class Program
    {
        static void Main(string[] args)
        {
            var id = Guid.NewGuid().ToString();

            using (var resourceManager = new ResourceManager(id))
            {
                foreach (var r in ProtectedResourcesExample.All)
                {
                    resourceManager.ProtectResource(r);
                }
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
