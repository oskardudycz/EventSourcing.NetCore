namespace ECommerce.Domain.Products.Contracts.Requests;

public record UpdateProductRequest(
    string Name,
    string? Description,
    string ProducerName,
    string? AdditionalInfo
)
{
    public Guid Id { get; set; }
};
