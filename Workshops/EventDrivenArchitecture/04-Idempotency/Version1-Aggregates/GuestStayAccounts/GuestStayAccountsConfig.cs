    using Idempotency.Core;

    namespace Idempotency.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;

public static class GuestStayAccountsConfig
{
    public static void ConfigureGuestStayAccounts(
        MessageBus messageBus,
        GuestStayFacade guestStayFacade
    )
    {
        messageBus.Subscribe<CheckOutGuest>(guestStayFacade.CheckOutGuest);
    }
}
