using BusinessProcesses.Core;
using BusinessProcesses.Version2_ImmutableEntities.ProcessManagers.GroupCheckouts;
using BusinessProcesses.Version2_ImmutableEntities.ProcessManagers.GuestStayAccounts;

namespace BusinessProcesses.Version2_ImmutableEntities.ProcessManagers.GuestStayAccounts;

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
