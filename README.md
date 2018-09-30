# rabbit-semaphore
Locking a distributed resources using RabbitMQ

0) RabbitMq is running
1) Configure appsettings (in StartUp and Consumer) with RabbitMQ credentials
2) Run "StartUp" once, to define what resources to lock
3) Start up multiple instances of "Consumer". It will select some resource to work on randomly.
4) Close a "working" consumer to see another one picking up the work.