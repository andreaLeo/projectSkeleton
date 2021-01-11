using System;
using System.Collections.Concurrent;

namespace Domain.Infrastructure.Serialization
{
    public interface ISerializationTypeProvider : IDisposable
    {
        BlockingCollection<Type> InstanciedTypes { get; }
    }
}
