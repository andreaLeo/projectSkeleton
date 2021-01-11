using Domain.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism;
using Prism.Ioc;
using Skeleton.Ninject;
using Skeleton.WPF.Prism.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Skeleton.WPF.Prism
{
    public abstract class SkeletonApplicationBase : PrismApplicationBase
    {
        protected override IContainerExtension CreateContainerExtension()
        {
            var container = new ContainerExtension(new NInjectContainer());
            LoadModule(container.Instance);
            return container;
        }
        protected abstract void LoadModule(IDependencyContainer container);

    }
}
