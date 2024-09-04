    using Idempotency.Core;

    namespace Idempotency.Sagas.Version1_Aggregates.GuestStayAccounts;

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
