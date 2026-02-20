using BusinessProcesses.Core;

namespace BusinessProcesses.ProcessManagers.GuestStayAccounts;

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
