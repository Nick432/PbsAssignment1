using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

 
// test

class Program
{
    static void Main(string[] args)
    {
       

        var username = args[0];
        var middlewareEndpoint = args[1];

        var factory = new ConnectionFactory() { HostName = middlewareEndpoint };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())


        {
            channel.ExchangeDeclare(exchange: "room", type: "topic");


            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName, exchange: "room", routingKey: "");


            var consumer = new EventingBasicConsumer(channel);
            
            consumer.Received += (model, ea) =>

            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"{ea.RoutingKey}: {message}");
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            Console.WriteLine($"[*] Started listening to room topic. Your username is: {username}");
            Console.WriteLine("Enter your message (type 'exit' to quit):");

            while (true)
            {
                var message = Console.ReadLine();
                if (message.ToLower() == "exit")
                    break;

                var body = Encoding.UTF8.GetBytes($"{username}: {message}");
                channel.BasicPublish(exchange: "room", routingKey: "", basicProperties: null, body: body);
            }
        }
    }
}