using Core.Validation;

namespace Reservations.Guests.GettingGuestByExternalId;

public record GetGuestIdByExternalId
(
    GuestExternalId ExternalId
)
{
    public static ValueTask<GuestId> Query(GetGuestIdByExternalId query, CancellationToken ct)
    {
        // Here you'd probably call some external module
        // Or even orchestrate creation if it doesn't exist already
        // But I'm just doing dummy mapping
        return new ValueTask<GuestId>(new GuestId(query.ExternalId.Value));
    }
}
