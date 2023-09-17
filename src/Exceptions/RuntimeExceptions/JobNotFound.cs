namespace FluentQueue.Exceptions.RuntimeExceptions;

using FluentQueue.Exceptions;

public class JobNotFound : RuntimeException
{
    public JobNotFound() : base(message: "Job not found! pls check message and job's dto.")
    { }
}
