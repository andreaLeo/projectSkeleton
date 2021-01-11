using Skeleton.WPF.Prism.Interfaces.Reflection;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Skeleton.Services.Reflection
{
    public class ObjectReflectionPropertyDescriptor : PropertyDescriptor
    {
        private readonly PropertyInfo _propertyInfo;

        public ObjectReflectionPropertyDescriptor(
            Type componentType,
            PropertyInfo propertyInfo,
            Attribute[] attrs)
            : base(propertyInfo.Name, attrs)
        {
            ComponentType = componentType;
            _propertyInfo = propertyInfo;
        }

        private string _displayName;
        public override string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(_displayName))
                {
                    return _displayName;
                }
                var attr = Attributes[typeof(DisplayNameAttribute)];
                if (attr is DisplayNameAttribute displayName
                   && !attr.IsDefaultAttribute())
                {
                    _displayName = displayName.DisplayName;
                }
                else
                {
                    _displayName = base.DisplayName;
                }

                return _displayName;
            }
        }

        public override bool CanResetValue(object component) => false;

        public override object GetValue(object component)
        {
            if (component is ISingleObjectWrapper wrapper)
            {
                wrapper.GetProperty(Name, out object result);
                return result;
            }

            return null;
        }

        public override void ResetValue(object component) => throw new NotImplementedException();

        public override void SetValue(object component, object value) => ((ISingleObjectWrapper)component).SetProperty(Name, value);

        public override bool ShouldSerializeValue(object component) => throw new NotImplementedException();

        public override Type ComponentType { get; }
        public override bool IsReadOnly
        {
            get
            {
                var attr = this.Attributes[typeof(ReadOnlyAttribute)];
                if (attr is ReadOnlyAttribute readOnlyAttribute
                    && !readOnlyAttribute.IsDefaultAttribute())
                {
                    return readOnlyAttribute.IsReadOnly;
                }
                return false;
            }
        }
        public override Type PropertyType => _propertyInfo.PropertyType;
    }
}
