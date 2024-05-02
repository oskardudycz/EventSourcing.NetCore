using Core.Serialization.Newtonsoft;
using Marten;
using Marten.Services.Json;
using Newtonsoft.Json.Serialization;

namespace Core.Marten.Serialization;

public class NonDefaultConstructorMartenJsonNetContractResolver(
    Casing casing,
    CollectionStorage collectionStorage,
    NonPublicMembersStorage nonPublicMembersStorage = NonPublicMembersStorage.Default)
    : JsonNetContractResolver(casing, collectionStorage, nonPublicMembersStorage)
{
    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        return JsonObjectContractProvider.UsingNonDefaultConstructor(
            base.CreateObjectContract(objectType),
            objectType,
            base.CreateConstructorParameters
        );
    }
}
