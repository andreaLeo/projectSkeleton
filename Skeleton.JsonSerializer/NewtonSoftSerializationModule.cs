using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.DependencyInjection.Extensions;
using Domain.Infrastructure.Serialization;
using Domain.Infrastructure.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skeleton.Serialization.NewtonSoft
{
    class NewtonSoftSerializationModule : IInfrastructureModule
    {
        public IServiceCollection Bind(IServiceCollection serviceCollection) =>
            serviceCollection.Add(new[] { typeof(ISerializer), typeof(IJsonSerializer) }, typeof(NewtonSoftSerializer));
    }
}
