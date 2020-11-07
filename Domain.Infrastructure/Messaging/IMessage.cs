using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Infrastructure.Messaging
{
    public interface IMessage
    {
        Guid Id { get; }
    }
}
