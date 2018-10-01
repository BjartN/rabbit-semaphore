# rabbit-semaphore

Locking a distributed resources using RabbitMQ

0. RabbitMq is running
1. Configure appsettings (in StartUp and Consumer) with RabbitMQ credentials
2. Run "StartUp" once. This will define one queue per protected resource in RabbitMQ.
3. Start up multiple instances of "Consumer". It will select some resource to work on randomly.
4. Close a "working" consumer to see another one picking up the work.

https://www.rabbitmq.com/blog/2014/02/19/distributed-semaphores-with-rabbitmq/
