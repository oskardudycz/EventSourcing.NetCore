using BusinessProcesses.Core;

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
