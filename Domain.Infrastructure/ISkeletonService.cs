using System;
using Domain.Infrastructure.DependencyInjection;

namespace Domain.Infrastructure
{
    public interface ISkeletonService : IDisposable
    {
        bool Initialize(IDependencyResolver resolver);
    }
}
