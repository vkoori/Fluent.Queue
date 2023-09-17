namespace FluentQueue.Implementation.Connection;

public abstract class BaseBusConnectionDtoAbstract
{
    public abstract string HostName { get; set; }
    public abstract short Port { get; set; }
    public abstract string UserName { get; set; }
    public abstract string Password { get; set; }
}
