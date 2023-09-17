namespace FluentQueue.Exceptions.RuntimeExceptions;

using FluentQueue.Exceptions;

public class InvalidQueueType : RuntimeException
{
    public InvalidQueueType() : base(message: "This driver does not support the queue type you sent. Modify the queue type.")
    { }
}
