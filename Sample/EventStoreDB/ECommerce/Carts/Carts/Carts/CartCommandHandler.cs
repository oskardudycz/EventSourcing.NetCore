using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Carts.Carts.Commands;
using Carts.Pricing;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace Carts.Carts
{
    internal class CartCommandHandler:
        ICommandHandler<InitCart>,
        ICommandHandler<AddProduct>,
        ICommandHandler<RemoveProduct>,
        ICommandHandler<ConfirmCart>
    {
        private readonly IRepository<Cart> cartRepository;
        private readonly IProductPriceCalculator productPriceCalculator;

        public CartCommandHandler(
            IRepository<Cart> cartRepository,
            IProductPriceCalculator productPriceCalculator)
        {
            Guard.Against.Null(cartRepository, nameof(cartRepository));
            Guard.Against.Null(productPriceCalculator, nameof(productPriceCalculator));

            this.cartRepository = cartRepository;
            this.productPriceCalculator = productPriceCalculator;
        }

        public async Task<Unit> Handle(InitCart command, CancellationToken cancellationToken)
        {
            var cart = Cart.Initialize(command.CartId, command.ClientId);

            await cartRepository.Add(cart, cancellationToken);

            return Unit.Value;
        }

        public Task<Unit> Handle(AddProduct command, CancellationToken cancellationToken)
        {
            return cartRepository.GetAndUpdate(
                command.CartId,
                cart => cart.AddProduct(productPriceCalculator, command.ProductItem),
                cancellationToken);
        }

        public Task<Unit> Handle(RemoveProduct command, CancellationToken cancellationToken)
        {
            return cartRepository.GetAndUpdate(
                command.CartId,
                cart => cart.RemoveProduct(command.ProductItem),
                cancellationToken);
        }

        public Task<Unit> Handle(ConfirmCart command, CancellationToken cancellationToken)
        {
            return cartRepository.GetAndUpdate(
                command.CartId,
                cart => cart.Confirm(),
                cancellationToken);
        }
    }
}
