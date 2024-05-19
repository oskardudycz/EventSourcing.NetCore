using Newtonsoft.Json.Serialization;

namespace Core.Serialization.Newtonsoft;

public class NonDefaultConstructorContractResolver: DefaultContractResolver
{
    protected override JsonObjectContract CreateObjectContract(Type objectType) =>
        JsonObjectContractProvider.UsingNonDefaultConstructor(
            base.CreateObjectContract(objectType),
            objectType,
            base.CreateConstructorParameters
        );
}
