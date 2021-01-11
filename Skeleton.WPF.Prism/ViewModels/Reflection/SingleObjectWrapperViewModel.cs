using Domain.Infrastructure.Reflection;
using Skeleton.Services.Reflection;
using Skeleton.WPF.Prism.Interfaces.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.WPF.Prism.ViewModels.Reflection
{
    public class SingleObjectWrapperViewModel : DynamicObject, IObjectWrapperViewModel
    {
        protected readonly ConcurrentDictionary<string, object> _data = new ConcurrentDictionary<string, object>();
        protected IObjectReflectionAccessService _reflectionService;
        private readonly ConcurrentQueue<string> _batchEdit = new ConcurrentQueue<string>();

        private volatile bool _isEditing = false;

        public SingleObjectWrapperViewModel(object wrappedObject,
            IObjectReflectionAccessService reflectionService)
        {
            _reflectionService = reflectionService;
            WrappedObject = wrappedObject;
            // _reflectionService.RegisterViewModelPropertyDescriptor(GetCustomType(), this);
        }

        public virtual bool GetProperty(string propertyName, out object result)
        {
            if (!_data.TryGetValue(propertyName, out result))
            {
                return _reflectionService.Read(_wrappedObject, propertyName, out result);
            }
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) => GetProperty(binder.Name, out result);

        public override string ToString() => WrappedObject.ToString();

        public override int GetHashCode() => WrappedObject.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is IObjectWrapperViewModel viewModel)
            {
                return WrappedObject.Equals(viewModel.WrappedObject);
            }
            return WrappedObject.Equals(obj);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) => SetProperty(binder.Name, value);

        public override IEnumerable<string> GetDynamicMemberNames() => _data.Keys.Concat(GetProperties().Cast<PropertyDescriptor>().Select(descriptor => descriptor.Name)).Distinct();

        [Browsable(false)]
        public Guid UniqueId { get; } = Guid.NewGuid();

        private object _wrappedObject = null;
        [Browsable(false)]
        public object WrappedObject
        {
            get => _wrappedObject;
            set => SetProperty(ref _wrappedObject, value, OnWrappedObjectChanged);
        }

        protected virtual void OnWrappedObjectChanged()
        {
        }

        public virtual object this[string propertyName]
        {
            get
            {
                GetProperty(propertyName, out object result);
                return result;
            }
            set => SetProperty(propertyName, value);
        }

        public bool IsFullyBuilt { get; private set; }

        public virtual bool SetProperty(
            string propertyName,
            object value,
            Action onChanged = null)
        {

            if (GetProperty(propertyName, out object result))
            {
                if (object.Equals(result, value))
                    return false;
            }

            OnPropertyChanging(propertyName);
            SetDynamicProperty(propertyName, value);

            OnPropertyChanged(propertyName);

            if (_isEditing && IsFullyBuilt)
            {
                _batchEdit.Enqueue(propertyName);
            }

            return true;
        }

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(WrappedObject, true);
        public virtual string GetClassName() => TypeDescriptor.GetClassName(WrappedObject, true);

        public virtual string GetComponentName() => TypeDescriptor.GetComponentName(WrappedObject, true);

        public virtual TypeConverter GetConverter() => TypeDescriptor.GetConverter(WrappedObject, true);

        public virtual EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(WrappedObject, true);

        public virtual PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(WrappedObject, true);

        public virtual object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(WrappedObject, editorBaseType, true);

        public virtual EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(WrappedObject, true);

        public virtual EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(WrappedObject, attributes, true);

        public virtual PropertyDescriptorCollection GetProperties() => _reflectionService.GetPropertiesDescriptors(GetCustomType());

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => TypeDescriptor.GetProperties(this, attributes, true);

        public virtual object GetPropertyOwner(PropertyDescriptor pd) => this;

        public TypeCode GetTypeCode() => TypeCode.Object;

        public virtual Type GetCustomType() => WrappedObject.GetType();

        protected virtual void SetDynamicProperty(string propertyName, object value)
        {
            var props = _reflectionService.GetPropertiesDescriptors(GetCustomType());
            if (props.Count != 0)
            {
                var descriptor = props.OfType<ObjectReflectionPropertyDescriptor>().FirstOrDefault(c => c.Name == propertyName);
                if (descriptor == null)
                {
                    _data[propertyName] = value;
                }
                else if (descriptor != null
                    && descriptor.PropertyType != value.GetType())
                {
                    _data[propertyName] = value;

                    if (value is IObjectWrapperViewModel viewModel)
                    {
                        _reflectionService.Write(WrappedObject, propertyName, viewModel.WrappedObject);
                    }
                }
                else
                {
                    _reflectionService.Write(WrappedObject, propertyName, value);
                }
            }
        }

        protected virtual bool SetProperty<T>(
           ref T storage,
           T value,
           Action onChanged = null,
           [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            OnPropertyChanging(propertyName);
            storage = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);

            return true;
        }


        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_isEditing)
            {
                _batchEdit.Enqueue(propertyName);
                return;
            }

            var args = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, args);
        }

        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            if (_isEditing)
                return;
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }
        #endregion
    }
}
