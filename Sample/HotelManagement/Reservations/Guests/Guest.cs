using Core.Validation;

namespace Reservations.Guests;

public record GuestExternalId(string Value)
{
    public static GuestExternalId FromPrefix(string prefix, string externalId) =>
        new($"{prefix.NotEmpty()}/{externalId.NotEmpty()}");
}

public record GuestId(string Value);
