using Core.Validation;

namespace Reservations.Guests;

public record GuestExternalId(string Value)
{
    public static GuestExternalId FromPrefix(string prefix, string externalId) =>
        new($"{prefix.AssertNotEmpty()}/{externalId.AssertNotEmpty()}");
}

public record GuestId(string Value);
