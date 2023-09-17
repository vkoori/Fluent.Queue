namespace FluentQueue.Implementation.Drivers.RabbitMq.Connection;

using FluentQueue.Implementation.Connection;
using RabbitMQ.Client;

public class RabbitMqConnectionBuilder : ConnectionBuilderAbstract<IConnection, RabbitMqConnectionDto>
{
    public override IConnection CreateConnection(RabbitMqConnectionDto connectionDto)
    {
        var factory = new ConnectionFactory()
        {
            HostName = connectionDto.HostName,
            Port = connectionDto.Port,
            UserName = connectionDto.UserName,
            Password = connectionDto.Password,
            VirtualHost = connectionDto.VirtualHost,
            MaxMessageSize = connectionDto.MaxMessageSize
        };

        return factory.CreateConnection();
    }
}
