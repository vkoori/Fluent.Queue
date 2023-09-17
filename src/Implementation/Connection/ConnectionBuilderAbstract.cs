namespace FluentQueue.Implementation.Connection;

using System;

public abstract class ConnectionBuilderAbstract<TConnection, TConnectionDto>
    where TConnection : IDisposable
    where TConnectionDto : BaseBusConnectionDtoAbstract
{
    public abstract TConnection CreateConnection(TConnectionDto connectionDto);
}
