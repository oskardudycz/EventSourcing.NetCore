# Exercise 10 - Optimistic Concurrency with Marten

Having the following shopping cart process and implementation from previous exercises:
1. The customer may add a product to the shopping cart only after opening it.
2. When selecting and adding a product to the basket customer needs to provide the quantity chosen. The product price is calculated by the system based on the current price list.
3. The customer may remove a product with a given price from the cart.
4. The customer can confirm the shopping cart and start the order fulfilment process.
5. The customer may also cancel the shopping cart and reject all selected products.
6. After shopping cart confirmation or cancellation, the product can no longer be added or removed from the cart.

How will the solution change when we add requirements:
1. We can add up to 10 products marked as "Super discount!".
2. The customer can only have one open cart at a time.

How will we ensure data consistency when, for example, the wife and husband have access to the account, who at the same time tried to open the basket, or the husband confirmed and the wife tried to add another purchase?

Extend your implementation to include these rules.

![events](./assets/events.jpg)

There are two variations:
1. Immutable, with functional command handlers composition and entities as anemic data model: [Immutable/OptimisticConcurrencyTests.cs](./Immutable/BusinessLogicTests.cs).
2. Classical, mutable aggregates (rich domain model): [Mutable/BusinessLogicTests.cs](./Mutable/BusinessLogicTests.cs).
3. Mixed approach, mutable aggregates (rich domain model), returning events from methods, using immutable DTOs: [Mixed/BusinessLogicTests.cs](./Mixed/BusinessLogicTests.cs).

Select your preferred approach (or all) to solve this use case using Marten. Fill appropriate `DocumentSessionExtensions`

_**Note**: If needed update entities, events or test setup structure_


