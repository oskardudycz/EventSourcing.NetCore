using BusinessProcesses.Core;

namespace BusinessProcesses.Sagas.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;

public static class GuestStayAccountsConfig
{
    public static void ConfigureGuestStayAccounts(
        EventStore eventStore,
        GuestStayFacade guestStayFacade
    )
    {
        eventStore.Subscribe<CheckOutGuest>(guestStayFacade.CheckOutGuest);
    }
}
