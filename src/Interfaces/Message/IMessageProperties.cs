namespace FluentQueue.Interfaces.Message;

public interface IMessageProperties
{
    string? CorrelationId { get; set;}
    DateTime? Expiration { get; set;}
}
