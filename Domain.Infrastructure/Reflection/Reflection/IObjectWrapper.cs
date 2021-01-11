using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.WPF.Prism.Interfaces.Reflection
{
    public interface IObjectWrapper : ICustomTypeProvider
    {
        Guid UniqueId { get; }
        object WrappedObject { get; set; }
    }

    public interface ISingleObjectWrapper : IObjectWrapper,
        ICustomTypeDescriptor
    {
        bool GetProperty(string propertyName, out object result);
        bool SetProperty(string propertyName, object value, Action onChanged = null);
        object this[string propertyName] { get; set; }
    }
}
