namespace FluentQueue.Exceptions.RuntimeExceptions;

using FluentQueue.Exceptions;

public class ProducerNotFound : RuntimeException
{
    public ProducerNotFound() : base(message: "Producer not found! pls check your connection setting and your message.")
    { }
}
