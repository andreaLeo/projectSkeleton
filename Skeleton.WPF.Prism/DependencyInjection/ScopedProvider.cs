using Domain.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skeleton.WPF.Prism.DependencyInjection
{
    public class ScopedProvider : IScopedProvider
    {
        public bool IsAttached { get; set; }

        public IScopedProvider CurrentScope { get; }

        private readonly IDependencyResolver _resolver;

        public ScopedProvider(IDependencyResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public void Dispose() => _resolver.Dispose();

        public object Resolve(Type type) => _resolver.Resolve(type);
        public object Resolve(Type type, params (Type Type, object Instance)[] parameters) => _resolver.Resolve(type, parameters);
        public object Resolve(Type type, string name) => _resolver.Resolve(type, name);

        public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters) => _resolver.Resolve(type, name, parameters);

        public IScopedProvider CreateScope() => CurrentScope.CreateScope();
    }
}
