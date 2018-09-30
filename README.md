# rabbit-semaphore

Locking a distributed resources using RabbitMQ

0. RabbitMq is running
1. Configure appsettings (in StartUp and Consumer) with RabbitMQ credentials
1. Run "StartUp" once, to define what resources to lock
1. Start up multiple instances of "Consumer". It will select some resource to work on randomly.
1. Close a "working" consumer to see another one picking up the work.

https://www.rabbitmq.com/blog/2014/02/19/distributed-semaphores-with-rabbitmq/
