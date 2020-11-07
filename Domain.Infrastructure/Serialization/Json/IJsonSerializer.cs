namespace Domain.Infrastructure.Serialization.Json
{
    /// <summary />
    public interface IJsonSerializer : ISerializer
    {
        /// <summary />
        dynamic StringToDynamic(string toDeserialize);

        /// <summary />
        void SetCustomContractResolver<T>()
            where T : class, IJsonCustomContract, new();

        /// <summary />
        void AddCustomJsonConverter<T>()
            where T : class, IJsonCustomConverter, new();

        /// <summary />
        void UseCamelCaseContractResolver();
    }
}
