namespace FluentQueue.Exceptions.RuntimeExceptions;

using FluentQueue.Exceptions;

public class SubscriberNotFound : RuntimeException
{
    public SubscriberNotFound() : base(message: "Subscriber not found! pls check your connection setting and your queue.")
    { }
}
