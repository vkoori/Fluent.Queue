namespace FluentQueue.Interfaces.Drivers.RabbitMQ.Message;

using FluentQueue.Interfaces.Message;

public interface IRabbitMqMessage : IMessage
{
    IExchange? Exchange { get; set; }
    string? RoutingKey { get; set; }
}
