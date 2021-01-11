using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.WPF.Prism.Interfaces.Reflection
{
    public interface IObjectWrapperViewModel : ISingleObjectWrapper,
         ICustomTypeDescriptor,
        INotifyPropertyChanged,
        INotifyPropertyChanging
    {
        //void BeginEdit();
        bool IsFullyBuilt { get; }
        //void EndEdit();
        //PropertyDescriptorCollection FilterPropertiesDescriptor();
    }


    public interface ICollectionWrapperViewModel : ICollectionView,
        IObjectWrapper
    { }

    public interface IDictionaryWrapperViewModel : ICollectionView,
        IObjectWrapper
    { }
}
