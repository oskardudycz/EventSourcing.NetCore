using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using Microsoft.EntityFrameworkCore;
using Shipments.Packages.Events.External;
using Shipments.Packages.Requests;
using Shipments.Products;
using Shipments.Storage;

namespace Shipments.Packages
{
    public interface IPackageService
    {
        Task<Package> SendPackage(SendPackage request, CancellationToken cancellationToken = default);
        Task DeliverPackage(DeliverPackage request, CancellationToken cancellationToken = default);
        Task<Package> GetById(Guid id);
    }

    internal class PackageService: IPackageService
    {
        private readonly ShipmentsDbContext dbContext;
        private readonly IEventBus eventBus;
        private readonly IProductAvailabilityService productAvailabilityService;

        private DbSet<Package> Packages => dbContext.Packages;

        public PackageService(
            ShipmentsDbContext dbContext,
            IEventBus eventBus,
            IProductAvailabilityService productAvailabilityService
        )
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this.productAvailabilityService = productAvailabilityService ??
                                              throw new ArgumentNullException(nameof(productAvailabilityService));
        }

        public Task<Package> GetById(Guid id)
        {
            return dbContext.Packages
                .Include(p => p.ProductItems)
                .SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Package> SendPackage(SendPackage request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.ProductItems?.Count == 0)
                throw new ArgumentException("It's not possible to send package with empty product items");

            if (!productAvailabilityService.IsEnoughOf(request.ProductItems!.ToArray()))
            {
                await Publish(new ProductWasOutOfStock(request.OrderId, DateTime.UtcNow));
                throw new ArgumentOutOfRangeException(nameof(request.ProductItems), "Cannot send package - product was out of stock.");
            }

            var package = new Package
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                ProductItems = request.ProductItems.Select(pi =>
                    new ProductItem {Id = Guid.NewGuid(), ProductId = pi.ProductId, Quantity = pi.Quantity}).ToList(),
                SentAt = DateTime.Now
            };

            await Packages.AddAsync(package, cancellationToken);

            var @event = new PackageWasSent(package.Id,
                package.OrderId,
                package.ProductItems,
                package.SentAt);

            await SaveChangesAndPublish(@event, cancellationToken);

            return package;
        }

        public async Task DeliverPackage(DeliverPackage request, CancellationToken cancellationToken = default)
        {
            var package = await Packages.FindAsync(request.Id, cancellationToken);

            Packages.Update(package);

            await SaveChanges(cancellationToken);
        }

        private async Task SaveChangesAndPublish(IEvent @event, CancellationToken cancellationToken = default)
        {
            await SaveChanges(cancellationToken);

            await Publish(@event);
        }

        private async Task Publish(IEvent @event)
        {
            await eventBus.Publish(@event);
        }

        private Task SaveChanges(CancellationToken cancellationToken = default)
        {
            return dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
