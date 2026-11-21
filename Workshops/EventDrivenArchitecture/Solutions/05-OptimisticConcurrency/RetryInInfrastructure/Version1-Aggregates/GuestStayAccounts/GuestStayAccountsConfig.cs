    using OptimisticConcurrency.Core;

namespace OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;

public static class GuestStayAccountsConfig
{
    public static void ConfigureGuestStayAccounts(
        MessageBus messageBus,
        GuestStayAccountFacade guestStayFacade
    )
    {
        messageBus.Subscribe<CheckOutGuest>(guestStayFacade.CheckOutGuest);
    }
}
