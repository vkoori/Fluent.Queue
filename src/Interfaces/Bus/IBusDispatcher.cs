namespace FluentQueue.Interfaces.Bus;

using System;
using FluentQueue.Interfaces.Queue;

public interface IBusDispatcher
{
    IBusDispatcher OnConnection(string connection);
    IBusDispatcher OnQueue(IQueue queue);
    IBusDispatcher OnQueue(List<IQueue> queue);
    IBusDispatcher OnDelay(DateTime availableAt);
    void Dispatch();
}
