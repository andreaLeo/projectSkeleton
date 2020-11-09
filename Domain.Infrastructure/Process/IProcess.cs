using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Infrastructure.Process
{
    public interface IProcess : IDisposable
    {
        void Start(string[] args);
        void Stop();
    }
}
