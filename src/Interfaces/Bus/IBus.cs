namespace FluentQueue.Interfaces.Bus;

using FluentQueue.Interfaces.Message;

public interface IBus
{
    IBusDispatcher Message(IMessage message);
}
