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
#endregion

#region Publish/Subscribe()Pub/Sub) Tasarımı
//string exchangeName = "example-pub-sub-exchange";
//await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Fanout);
//byte[] message = Encoding.UTF8.GetBytes("HelloRabbitMQ");
//await channel.BasicPublishAsync(exchangeName, string.Empty, message);
#endregion

#region Work Queue(İş Kuyruğu Tasarımı)
//string queueName = "example-work-queue";
//await channel.QueueDeclareAsync(queueName, false, false, false);

//for (int i = 0; i < 100; i++)
//{
//    await Task.Delay(200);
//    byte[] message = Encoding.UTF8.GetBytes("Merhaba" + i);
//    await channel.BasicPublishAsync(exchange: string.Empty, queueName, message);
//}
#endregion

#region Request-Response Tasarımı
string requestQueueName = "example-request-response-queue";
await channel.QueueDeclareAsync(requestQueueName, false, false, false);

//Random isim oluşturulacak
var ReplyQueueName = await channel.QueueDeclareAsync();
var responseQueueName = ReplyQueueName.QueueName;
//unique bir değer
string correlationId = Guid.NewGuid().ToString();

#region Request Mesajını Oluşturma Ve Gönderme Davranışı

//IBasicProperties properties = new BasicProperties
//{
//    ReplyTo = responseQueueName,
//    CorrelationId = correlationId,
//};
var properties = new BasicProperties();
properties.CorrelationId = correlationId;
properties.ReplyTo = responseQueueName;

for (int i = 0; i < 10; i++)
{
    byte[] message = Encoding.UTF8.GetBytes("Merhaba" + i);
    await channel.BasicPublishAsync(exchange: string.Empty,
        routingKey: requestQueueName,
        basicProperties: properties,
        body: message,
        mandatory: true
        );
}
#endregion

#region Response Kuyruğu Dinleme Davranışı
AsyncEventingBasicConsumer consumer = new(channel);
await channel.BasicConsumeAsync(queue: responseQueueName, autoAck: true, consumer);
consumer.ReceivedAsync += async (sender, e) =>
{
    if (e.BasicProperties.CorrelationId == correlationId)
    {
        Console.WriteLine($"Response : {Encoding.UTF8.GetString(e.Body.Span)}");
        await Task.CompletedTask;
    }
};
#endregion

#endregion

Console.Read();