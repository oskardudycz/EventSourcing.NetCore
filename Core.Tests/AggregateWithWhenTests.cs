using Core.Aggregates;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

public record Person(
    string Name,
    string Address
);

public record InvoiceInitiated(
    double Amount,
    string Number,
    Person IssuedTo,
    DateTime InitiatedAt
);

public record InvoiceIssued(
    string IssuedBy,
    DateTime IssuedAt
);

public enum InvoiceSendMethod
{
    Email,
    Post
}

public record InvoiceSent(
    InvoiceSendMethod SentVia,
    DateTime SentAt
);

public enum InvoiceStatus
{
    Initiated = 1,
    Issued = 2,
    Sent = 3
}

public class Invoice: Aggregate<string>
{
    public double Amount { get; private set; }
    public string Number { get; private set; } = default!;

    public InvoiceStatus Status { get; private set; }

    public Person IssuedTo { get; private set; } = default!;
    public DateTime InitiatedAt { get; private set; }

    public string? IssuedBy { get; private set; }
    public DateTime IssuedAt { get; private set; }

    public InvoiceSendMethod SentVia { get; private set; }
    public DateTime SentAt { get; private set; }

    public override void Evolve(object @event)
    {
        switch (@event)
        {
            case InvoiceInitiated invoiceInitiated:
                Apply(invoiceInitiated);
                break;
            case InvoiceIssued invoiceIssued:
                Apply(invoiceIssued);
                break;
            case InvoiceSent invoiceSent:
                Apply(invoiceSent);
                break;
        }
    }

    private void Apply(InvoiceInitiated @event)
    {
        Id = @event.Number;
        Amount = @event.Amount;
        Number = @event.Number;
        IssuedTo = @event.IssuedTo;
        InitiatedAt = @event.InitiatedAt;
        Status = InvoiceStatus.Initiated;
    }

    private void Apply(InvoiceIssued @event)
    {
        IssuedBy = @event.IssuedBy;
        IssuedAt = @event.IssuedAt;
        Status = InvoiceStatus.Issued;
    }

    private void Apply(InvoiceSent @event)
    {
        SentVia = @event.SentVia;
        SentAt = @event.SentAt;
        Status = InvoiceStatus.Sent;
    }
}

public class AggregateWithWhenTests
{
    [Fact]
    public void AggregationWithWhenShouldGetTheCurrentState()
    {
        var invoiceInitiated = new InvoiceInitiated(
            34.12,
            "INV/2021/11/01",
            new Person("Oscar the Grouch", "123 Sesame Street"),
            DateTime.UtcNow
        );
        var invoiceIssued = new InvoiceIssued(
            "Cookie Monster",
            DateTime.UtcNow
        );
        var invoiceSent = new InvoiceSent(
            InvoiceSendMethod.Email,
            DateTime.UtcNow
        );

        // 1. Get all events and sort them in the order of appearance
        var events = new object[] {invoiceInitiated, invoiceIssued, invoiceSent};

        // 2. Construct empty Invoice object
        var invoice = new Invoice();

        // 3. Apply each event on the entity.
        foreach (var @event in events)
        {
            invoice.Evolve(@event);
        }

        invoice.Id.Should().Be(invoiceInitiated.Number);
        invoice.Amount.Should().Be(invoiceInitiated.Amount);
        invoice.Number.Should().Be(invoiceInitiated.Number);
        invoice.Status.Should().Be(InvoiceStatus.Sent);

        invoice.IssuedTo.Should().Be(invoiceInitiated.IssuedTo);
        invoice.InitiatedAt.Should().Be(invoiceInitiated.InitiatedAt);

        invoice.IssuedBy.Should().Be(invoiceIssued.IssuedBy);
        invoice.IssuedAt.Should().Be(invoiceIssued.IssuedAt);

        invoice.SentVia.Should().Be(invoiceSent.SentVia);
        invoice.SentAt.Should().Be(invoiceSent.SentAt);
    }
}
