using System;

namespace Carts.Api.Requests.Carts;

public record InitCartRequest(
    Guid ClientId
);
