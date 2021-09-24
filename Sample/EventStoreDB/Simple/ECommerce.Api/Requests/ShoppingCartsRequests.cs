using System;

namespace ECommerce.Api.Requests
{
    public record InitializeShoppingCartRequest(
        Guid? ClientId
    );
}
