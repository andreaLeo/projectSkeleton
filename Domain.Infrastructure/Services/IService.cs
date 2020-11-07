using System;
using Domain.Infrastructure.DependencyInjection;

namespace Domain.Infrastructure.Services
{
    public interface IService : IDisposable
    {
        bool Initialize(IDependencyResolver resolver);
    }
}
