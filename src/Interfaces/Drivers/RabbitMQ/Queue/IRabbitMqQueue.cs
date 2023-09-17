namespace FluentQueue.Interfaces.Drivers.RabbitMQ.Queue;

using FluentQueue.Interfaces.Queue;

public interface IRabbitMqQueue : IQueue
{
    bool Durable { get; set; }
    bool Exclusive { get; set; }
    bool AutoDelete { get; set; }
    IDictionary<string, object>? Arguments { get; set; }
}
