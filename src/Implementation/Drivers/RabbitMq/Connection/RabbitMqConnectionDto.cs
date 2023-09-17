namespace FluentQueue.Implementation.Drivers.RabbitMq.Connection;

using FluentQueue.Implementation.Connection;
using RabbitMQ.Client;

public class RabbitMqConnectionDto : BaseBusConnectionDtoAbstract
{
    public override string HostName { get; set; } = "localhost";
    public override short Port { get; set; } = AmqpTcpEndpoint.UseDefaultPort;
    public override string UserName { get; set; } = ConnectionFactory.DefaultUser;
    public override string Password { get; set; } = ConnectionFactory.DefaultPass;
    public string VirtualHost { get; set; } = ConnectionFactory.DefaultVHost;
    public uint MaxMessageSize { get; set; } = 512 * 1024 * 1024;
}
