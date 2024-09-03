using BusinessProcesses.Core;
using BusinessProcesses.Version2_ImmutableEntities.Sagas.GroupCheckouts;
using BusinessProcesses.Version2_ImmutableEntities.Sagas.GuestStayAccounts;

namespace BusinessProcesses.Version2_ImmutableEntities.Sagas.GuestStayAccounts;

using static GuestStayAccountCommand;

public static class GuestStayAccountsConfig
{
    public static void ConfigureGuestStayAccounts(
        CommandBus commandBus,
        GuestStayFacade guestStayFacade
    )
    {
        commandBus.Handle<CheckOutGuest>(guestStayFacade.CheckOutGuest);
    }
}
