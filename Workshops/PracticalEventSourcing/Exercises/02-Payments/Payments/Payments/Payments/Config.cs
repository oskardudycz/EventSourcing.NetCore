using Payments.Payments.Commands;
using Core.Repositories;
using Core.Storage;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Payments.Payments.Events;

namespace Payments.Payments
{
    internal static class PaymentsConfig
    {
        internal static void AddPayments(this IServiceCollection services)
        {
            AddCommandHandlers(services);
            AddEventHandlers(services);
        }

        private static void AddCommandHandlers(IServiceCollection services)
        {

        }

        private static void AddEventHandlers(IServiceCollection services)
        {

        }

        internal static void ConfigurePayments(this StoreOptions options)
        {

        }
    }
}
