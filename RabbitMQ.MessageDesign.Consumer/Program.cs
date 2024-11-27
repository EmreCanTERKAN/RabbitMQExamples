using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
ConnectionFactory factory = new();
factory.Uri = new("amqps://dylxmqal:7_A9Vfo3nd5WpN62rVmsltBJBzUrG6SU@cow.rmq2.cloudamqp.com/dylxmqal");

var connection = await factory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();

#region P2P(Point-to-Point) Tasarımı
//string name = "example-p2p-queue";
//await channel.QueueDeclareAsync(queue: name, durable: false, exclusive: false, autoDelete: false);
//byte[] message = Encoding.UTF8.GetBytes("Hello RabbitMQ");
//await channel.BasicPublishAsync(exchange: string.Empty, routingKey: name, body: message);
//AsyncEventingBasicConsumer consumer = new(channel);
//await channel.BasicConsumeAsync(queue: name, false, consumer);

//consumer.ReceivedAsync += async (sender, e) =>
//{
//    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
//    await Task.CompletedTask;
//};
#endregion

#region Publish/Subscribe()Pub/Sub) Tasarımı
//string exchangeName = "example-pub-sub-exchange";
//await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Fanout);
//var name = await channel.QueueDeclareAsync();
//string queueName = name.QueueName;
//await channel.QueueBindAsync(queueName, exchangeName, string.Empty);
//AsyncEventingBasicConsumer consumer = new(channel);
//await channel.BasicConsumeAsync(queueName, false, consumer);
//consumer.ReceivedAsync += async (sender, e) =>
//{
//    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
//    await Task.CompletedTask;
//};
#endregion

#region Work Queue(İş Kuyruğu Tasarımı)
//string queueName = "example-work-queue";
//await channel.QueueDeclareAsync(queueName, false, false, false);
//AsyncEventingBasicConsumer consumer = new(channel);
//await channel.BasicConsumeAsync(queueName, true, consumer);
//await channel.BasicQosAsync(prefetchCount: 1, prefetchSize: 0, global: false);

//consumer.ReceivedAsync += async(sender, e) =>
//{
//    Console.WriteLine(Encoding.UTF8.GetString(e.Body.Span));
//    await Task.CompletedTask;
//};
#endregion

#region Request-Response Tasarımı
string requestQueueName = "example-request-response-queue";
await channel.QueueDeclareAsync(requestQueueName, false, false, false);
AsyncEventingBasicConsumer consumer = new(channel);
await channel.BasicConsumeAsync(requestQueueName, autoAck: true, consumer);

consumer.ReceivedAsync += async (sender, e) =>
{
    string message = Encoding.UTF8.GetString(e.Body.Span);
    Console.WriteLine(message);
    //...
    byte[] responseMessage = Encoding.UTF8.GetBytes($"İşlem tamamlandı :{message}");
    var properties = new BasicProperties();
    properties.CorrelationId = e.BasicProperties.CorrelationId;
    await channel.BasicPublishAsync(
        string.Empty,
        e.BasicProperties.ReplyTo!,
        mandatory: true,
        body: responseMessage,
        basicProperties: properties);
};

#endregion

Console.Read();