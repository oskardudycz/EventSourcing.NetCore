using System;

namespace Carts.Api.Requests.Carts;

public class ConfirmCartRequest
{
    public Guid CartId { get; set; }
}