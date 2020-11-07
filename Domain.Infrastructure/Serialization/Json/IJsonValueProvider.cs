namespace Domain.Infrastructure.Serialization.Json
{
    public interface IJsonValueProvider
    {
        /// <summary />
        void SetValue(object target, object value);

        /// <summary />
        object GetValue(object target);
    }
}
