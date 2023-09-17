namespace FluentQueue.Interfaces.Drivers.RabbitMQ.Message;

public interface IExchange
{
    string Name { get; set; }
    string Type { get; set; }
}
