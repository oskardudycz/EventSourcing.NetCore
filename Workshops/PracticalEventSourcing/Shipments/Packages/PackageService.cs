using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shipments.Packages.Commands;
using Shipments.Packages.Events.External;
using Shipments.Storage;

namespace Shipments.Packages
{
    internal interface IPackageService
    {
        Task<Unit> SendPackage(SentPackage request, CancellationToken cancellationToken = default);
        Task<Unit> DeliverPackage(DeliverPackage request, CancellationToken cancellationToken = default);
    }

    internal class PackageService: IPackageService
    {
        private readonly ShipmentsDbContext dbContext;
        private readonly IEventBus eventBus;

        private DbSet<Package> Packages => dbContext.Packages;

        public PackageService(
            ShipmentsDbContext dbContext,
            IEventBus eventBus)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public async Task<Unit> SendPackage(SentPackage request, CancellationToken cancellationToken = default)
        {
            var package = new Package
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                ProductItems = request.ProductItems.ToList(),
                SentAt = DateTime.Now
            };

            await Packages.AddAsync(package, cancellationToken);

            var @event = new PackageWasSent(
                package.Id,
                package.OrderId,
                package.ProductItems,
                package.SentAt
            );

            await SaveChangesAndPublish(@event, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> DeliverPackage(DeliverPackage request, CancellationToken cancellationToken = default)
        {
            var package = await Packages.FindAsync(request.Id, cancellationToken);

            Packages.Update(package);

            await SaveChanges(cancellationToken);

            return Unit.Value;
        }

        private async Task SaveChangesAndPublish(IEvent @event, CancellationToken cancellationToken = default)
        {
            await SaveChanges(cancellationToken);

            await eventBus.Publish(@event);
        }

        private Task SaveChanges(CancellationToken cancellationToken = default)
        {
            return dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
