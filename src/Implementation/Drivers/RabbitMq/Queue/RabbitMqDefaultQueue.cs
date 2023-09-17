namespace FluentQueue.Implementation.Drivers.RabbitMq.Queue;

using System.Collections.Generic;
using FluentQueue.Interfaces.Drivers.RabbitMQ.Queue;

public class RabbitMqDefaultQueue : IRabbitMqQueue
{
    public string QueueName { get; set; } = "default";
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; } = false;
    public bool AutoDelete { get; set; } = false;
    public IDictionary<string, object>? Arguments { get; set; } = null;
}
