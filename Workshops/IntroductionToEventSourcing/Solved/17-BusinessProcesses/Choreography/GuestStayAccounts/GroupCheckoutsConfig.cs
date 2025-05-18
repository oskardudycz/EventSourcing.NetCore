using BusinessProcesses.Core;

namespace BusinessProcesses.Choreography.GuestStayAccounts;

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
