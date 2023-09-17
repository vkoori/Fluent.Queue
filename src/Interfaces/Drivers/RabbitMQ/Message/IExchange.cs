namespace FluentQueue.Interfaces.Drivers.RabbitMQ.Message;

public interface IExchange
{
    string Name { get; set; }
    string Type { get; set; }
    bool Durable { get; set; }
    bool AutoDelete { get; set; }
    IDictionary<string, object>? Arguments { get; set; }
}
