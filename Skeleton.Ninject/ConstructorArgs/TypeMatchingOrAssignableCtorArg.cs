using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Planning.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skeleton.Ninject.ConstructorArgs
{
    public class TypeMatchingOrAssignableCtorArg : IConstructorArgument
    {
        private readonly Type _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMatchingConstructorArgument" /> class.
        /// </summary>
        /// <param name="type">The type of the argument to override.</param>
        /// <param name="valueCallback">The callback that will be triggered to get the parameter's value.</param>
        public TypeMatchingOrAssignableCtorArg(
            Type type,
            Func<IContext, ITarget, object> valueCallback)
            : this(type, valueCallback, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMatchingOrAssignableConstructorArgument" /> class.
        /// </summary>
        /// <param name="type">The type of the argument to override.</param>
        /// <param name="valueCallback">The callback that will be triggered to get the parameter's value.</param>
        /// <param name="shouldInherit">Whether the parameter should be inherited into child requests.</param>
        public TypeMatchingOrAssignableCtorArg(
            Type type,
            Func<IContext, ITarget, object> valueCallback,
            bool shouldInherit)
        {
            ValueCallback = valueCallback;
            ShouldInherit = shouldInherit;
            _type = type;
        }

        /// <inheritdoc />
        public string Name => $"{nameof(TypeMatchingOrAssignableCtorArg)}_{_type.FullName}";

        /// <inheritdoc />
        public bool ShouldInherit { get; }

        /// <summary>
        /// Gets or sets the callback that will be triggered to get the parameter's value.
        /// </summary>
        private Func<IContext, ITarget, object> ValueCallback { get; }

        /// <inheritdoc />
        public bool AppliesToTarget(IContext context, ITarget target) => target.Type.IsAssignableFrom(_type);
        /// <inheritdoc />
        public object GetValue(IContext context, ITarget target) => ValueCallback(context, target);
        /// <inheritdoc />
        public bool Equals(IParameter other) => other is TypeMatchingOrAssignableCtorArg constructorArgument
                   && constructorArgument._type == _type;

        /// <inheritdoc />
        public override bool Equals(object obj) => !(obj is IParameter other) ? this == obj : Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => GetType().GetHashCode() ^ _type.GetHashCode();
    }
}
