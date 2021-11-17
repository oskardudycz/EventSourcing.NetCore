using System;
using System.Collections.Generic;
using Carts.Carts;
using Carts.Carts.InitializingCart;
using Core.Events;

namespace Carts.Tests.Builders;

internal class CartBuilder
{
    private readonly Queue<IEvent> eventsToApply = new();

    public CartBuilder Initialized()
    {
        var cartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        eventsToApply.Enqueue(new CartInitialized(cartId, clientId, CartStatus.Pending));

        return this;
    }

    public static CartBuilder Create() => new();

    public Cart Build()
    {
        var cart = (Cart) Activator.CreateInstance(typeof(Cart), true)!;

        foreach (var @event in eventsToApply)
        {
            cart.When(@event);
        }

        return cart;
    }
}