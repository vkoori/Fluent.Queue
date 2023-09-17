namespace FluentQueue.Interfaces.Message;

public interface IMessage
{
    object Body { get; set; }
    IMessageProperties? Properties { get; set; }
}
