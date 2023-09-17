namespace FluentQueue.Interfaces.Bus;

using FluentQueue.Interfaces.Queue;

public interface ISubscribe
{
    ISubscribe Subscribe(IQueue queue);
}
